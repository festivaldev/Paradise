using log4net;
using Newtonsoft.Json;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Paradise {
	public partial class TcpSocket {
		[Flags]
		public enum PayloadFlags {
			IsSerialized = 1 << 0,
			IsEncrypted = 1 << 1,
			IsOneWay = 1 << 2
		}

		public class Payload {
			private static readonly ILog Log = LogManager.GetLogger(nameof(Payload));

			public PacketType Type;
			public string Data;
			public ServerType ServerType;
			public PayloadFlags Flags;
			public Guid ConversationId;

			[JsonIgnore]
			public bool IsSerialized {
				get {
					return Flags.HasFlag(PayloadFlags.IsSerialized);
				}

				private set {
					if (value) {
						Flags |= PayloadFlags.IsSerialized;
					} else {
						Flags &= ~PayloadFlags.IsSerialized;
					}
				}
			}

			[JsonIgnore]
			public bool IsEncrypted {
				get {
					return Flags.HasFlag(PayloadFlags.IsEncrypted);
				}

				private set {
					if (value) {
						Flags |= PayloadFlags.IsEncrypted;
					} else {
						Flags &= ~PayloadFlags.IsEncrypted;
					}
				}
			}

			[JsonIgnore]
			public bool IsOneWay {
				get {
					return Flags.HasFlag(PayloadFlags.IsOneWay);
				}

				private set {
					if (value) {
						Flags |= PayloadFlags.IsOneWay;
					} else {
						Flags &= ~PayloadFlags.IsOneWay;
					}
				}
			}

			public static byte[] Encode(PacketType type, object data, RijndaelManaged crypto, out Payload payloadObj, bool oneWay = false, Guid conversationId = default, ServerType serverType = ServerType.None) {
				if (conversationId == default(Guid)) {
					conversationId = Guid.NewGuid();
				}

				payloadObj = new Payload {
					Type = type,
					ServerType = serverType,
					ConversationId = conversationId,
					IsOneWay = oneWay
				};

				using (var bytes = new MemoryStream()) {
					switch (type) {
						case PacketType.ClientInfo:
						case PacketType.ConnectionStatus:
							StringProxy.Serialize(bytes, JsonConvert.SerializeObject(data));
							break;
						case PacketType.Ping:
						case PacketType.Pong:
							StringProxy.Serialize(bytes, DateTime.UtcNow.ToString("o"));
							break;
						case PacketType.Command:
						case PacketType.Error:
						case PacketType.ChatMessage:
							payloadObj.IsEncrypted = true;

							StringProxy.Serialize(bytes, JsonConvert.SerializeObject(data));
							break;
						case PacketType.CommandOutput:
							payloadObj.IsEncrypted = true;

							StringProxy.Serialize(bytes, (string)data);
							break;
						case PacketType.Monitoring:
						case PacketType.BanPlayer:
							payloadObj.IsEncrypted = true;

							DictionaryProxy<string, object>.Serialize(bytes, (Dictionary<string, object>)data, StringProxy.Serialize, (stream, instance) => {
								StringProxy.Serialize(stream, JsonConvert.SerializeObject(instance));
							});
							break;
						case PacketType.PlayerJoined:
						case PacketType.PlayerLeft:
							payloadObj.IsEncrypted = true;

							CommActorInfoProxy.Serialize(bytes, (CommActorInfo)data);
							break;
						case PacketType.RoomOpened:
						case PacketType.RoomClosed:
							payloadObj.IsEncrypted = true;

							GameRoomDataProxy.Serialize(bytes, (GameRoomData)data);
							break;
						case PacketType.OpenRoom:
							break;
						case PacketType.CloseRoom:
							payloadObj.IsEncrypted = true;

							Int32Proxy.Serialize(bytes, (int)data);
							break;
						default: break;
					}

					if (crypto != null && payloadObj.IsEncrypted) {
						using (var memoryStream = new MemoryStream()) {
							using (var cryptoStream = new CryptoStream(memoryStream, crypto.CreateEncryptor(), CryptoStreamMode.Write)) {
								var _bytes = bytes.ToArray();
								cryptoStream.Write(_bytes, 0, _bytes.Length);
							}

							payloadObj.Data = Convert.ToBase64String(memoryStream.ToArray());
						}
					} else {
						payloadObj.Data = Convert.ToBase64String(bytes.ToArray());
					}
				}

				var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payloadObj)));
				return Encoding.UTF8.GetBytes(base64); ;
			}

			public static T Decode<T>(string base64, RijndaelManaged crypto, out Payload payloadObj) {
				if (string.IsNullOrWhiteSpace(base64)) {
					payloadObj = default(Payload);
					return default(T);
				}

				try {
					payloadObj = JsonConvert.DeserializeObject<Payload>(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
				} catch (Exception e) {
					Log.Info(base64);
					Log.Error(e);

					payloadObj = default(Payload);
					return default(T);
				}

				var data = Convert.FromBase64String(payloadObj.Data);

				if (crypto != null && payloadObj.IsEncrypted) {
					using (var memoryStream = new MemoryStream()) {
						using (var cryptoStream = new CryptoStream(memoryStream, crypto.CreateDecryptor(), CryptoStreamMode.Write)) {
							cryptoStream.Write(data, 0, data.Length);
						}

						data = memoryStream.ToArray();
					}
				}

				using (var bytes = new MemoryStream(data)) {
					var result = default(object);

					switch (payloadObj.Type) {
						case PacketType.ClientInfo:
							result = JsonConvert.DeserializeObject<SocketInfo>(StringProxy.Deserialize(bytes));
							break;
						case PacketType.ConnectionStatus:
							result = JsonConvert.DeserializeObject<SocketConnectionStatus>(StringProxy.Deserialize(bytes));
							break;
						case PacketType.Ping:
						case PacketType.Pong:
							result = DateTime.Parse(StringProxy.Deserialize(bytes)).ToUniversalTime();
							break;
						case PacketType.Command:
							result = JsonConvert.DeserializeObject<SocketCommand>(StringProxy.Deserialize(bytes));
							break;
						case PacketType.CommandOutput:
						case PacketType.Error:
						case PacketType.ChatMessage:
							result = StringProxy.Deserialize(bytes);
							break;
						case PacketType.Monitoring:
						case PacketType.BanPlayer:
							result = DictionaryProxy<string, object>.Deserialize(bytes, StringProxy.Deserialize, (stream) => {
								return JsonConvert.DeserializeObject<object>(StringProxy.Deserialize(stream));
							});
							break;
						case PacketType.PlayerJoined:
						case PacketType.PlayerLeft:
							result = CommActorInfoProxy.Deserialize(bytes);
							break;
						case PacketType.RoomOpened:
						case PacketType.RoomClosed:
							result = GameRoomDataProxy.Deserialize(bytes);
							break;
						case PacketType.OpenRoom:
							break;
						case PacketType.CloseRoom:
							result = Int32Proxy.Deserialize(bytes);
							break;
						default: break;
					}

					return (T)result;
				}
			}
		}
	}
}
