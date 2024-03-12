using log4net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UberStrike.Core.Serialization;

namespace Paradise {
	public partial class WebSocket {
		[Flags]
		public enum PayloadFlags {
			IsSerialized = 1 << 0,
			IsEncrypted = 1 << 1,
			IsOneWay = 1 << 2
		}

		public class Payload {
			protected static readonly ILog Log = LogManager.GetLogger(nameof(Payload));

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
					//return false;
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
				if (conversationId == default) {
					conversationId = Guid.NewGuid();
				}

				payloadObj = new Payload {
					Type = type,
					ServerType = serverType,
					ConversationId = conversationId,
					IsOneWay = oneWay,
				};

				using (var outputStream = new MemoryStream())
				using (var bytes = new MemoryStream()) {
					switch (type) {
						case PacketType.ClientInfo:
						case PacketType.ConnectionStatus:
							StringProxy.Serialize(bytes, JsonConvert.SerializeObject(data));
							break;

						case PacketType.CommandOutput:
							payloadObj.IsEncrypted = true;

							StringProxy.Serialize(bytes, (string)data);
							break;
						default:
							return null;
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

				return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payloadObj));
			}

			public static T Decode<T>(string json, RijndaelManaged crypto, out Payload payloadObj) {
				if (string.IsNullOrWhiteSpace(json)) {
					payloadObj = default;
					return default;
				}

				try {
					payloadObj = JsonConvert.DeserializeObject<Payload>(json);
				} catch (Exception e) {
					Log.Info(json);
					Log.Error(e);

					payloadObj = default;
					return default;
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
						default:
							break;
					}

					return (T)result;
				}
			}
		}
	}
}
