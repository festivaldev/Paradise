using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UberStrike.Core.Serialization;
using WebSocketSharp;

namespace Paradise {
	public partial class WebSocket {
		public class SocketClient {
			readonly byte[] MAGIC_BYTES = new byte[] { 0x65, 0x53, 0x69, 0x44, 0x61, 0x52, 0x61, 0x50 /* 0x50 */ };

			public const int CONNECT_TIMEOUT = 3;
			public const int RECONNECT_INTERVAL = 1;
			public const int MAX_RECONNECT = 10;
			public const int SEND_TIMEOUT = 3;
			public const int RECEIVE_TIMEOUT = 3;

			protected static readonly ILog Log = LogManager.GetLogger(nameof(SocketClient));

			private IPEndPoint RemoteEndPoint;
			private WebSocketSharp.WebSocket SocketConnection;
			private readonly ManualResetEvent ConnectionWaitHandle = new ManualResetEvent(false);

			private SocketInfo ClientInfo;
			private readonly RijndaelManaged CryptoProvider;

			public event EventHandler<SocketConnectedEventArgs> Connected;
			public event EventHandler<SocketDisconnectedEventArgs> Disconnected;
			public event EventHandler<SocketDataReceivedEventArgs> DataReceived;
			public event EventHandler<SocketConnectionRejectedEventArgs> ConnectionRejected;

			public SocketClient(Guid serverId, ServerType serverType, string encryptionPassphrase) {
				try {
					ClientInfo = new SocketInfo {
						SocketId = serverId,
						Type = serverType
					};

					if (!string.IsNullOrEmpty(encryptionPassphrase)) {
						CryptoProvider = new RijndaelManaged {
							KeySize = 256,
							Key = new Rfc2898DeriveBytes(encryptionPassphrase, ClientInfo.SocketId.ToByteArray()).GetBytes(32),
							IV = ClientInfo.SocketId.ToByteArray()
						};
					}
				} catch (Exception e) {
					Log.Error(e);
				}
			}

			public async void Connect(IPAddress ip, int port, int maxAttempts = MAX_RECONNECT) {
				RemoteEndPoint = new IPEndPoint(ip, port);

				var uriBuilder = new UriBuilder {
					Scheme = "ws",
					Host = RemoteEndPoint.Address.ToString(),
					Port = RemoteEndPoint.Port
				};

				SocketConnection = new WebSocketSharp.WebSocket(uriBuilder.ToString());

				SocketConnection.OnOpen += (sender, e) => {
					ConnectionWaitHandle.Set();
				};

				SocketConnection.OnClose += (sender, e) => {
					if ((e.Code == (ushort)CloseStatusCode.Abnormal && ConnectionWaitHandle.WaitOne(0)) || e.Code != (ushort)CloseStatusCode.Abnormal) {
						Disconnected?.Invoke(this, new SocketDisconnectedEventArgs {
							Socket = SocketConnection,
							Info = ClientInfo,
							Code = e.Code,
							Reason = e.Reason
						});
					}
				};

				SocketConnection.OnMessage += (sender, e) => {
					using (var inputStream = new MemoryStream(e.RawData))
					using (var outputStream = new MemoryStream()) {
						var payloadType = Int32Proxy.Deserialize(inputStream);

						if (payloadType == 0x42) { // Packet/Raw Data
							var packetType = EnumProxy<PacketType>.Deserialize(inputStream);

							switch (packetType) {
								case PacketType.MagicBytes:
									Int32Proxy.Serialize(outputStream, 0x42);
									EnumProxy<PacketType>.Serialize(outputStream, PacketType.MagicBytes);
									ArrayProxy<byte>.Serialize(outputStream, MAGIC_BYTES, ByteProxy.Serialize);

									SocketConnection.Send(outputStream.ToArray());
									break;
								case PacketType.ClientInfo:
									SocketConnection.Send(Payload.Encode(PacketType.ClientInfo, ClientInfo, null, out _, serverType: ClientInfo.Type));
									break;
								default:
									break;
							}
						} else { // JSON Object
							var payload = Payload.Decode<object>(Encoding.UTF8.GetString(e.RawData), CryptoProvider, out var payloadObj);

							switch (payloadObj.Type) {
								case PacketType.ConnectionStatus:
									var status = (SocketConnectionStatus)payload;
									Log.Debug($"Connected: {status.Connected}, Rejected: {status.Rejected}, Reason: {status.DisconnectReason}");

									if (status.Connected) {
										Connected?.Invoke(this, new SocketConnectedEventArgs {
											Socket = SocketConnection
										});

										SocketConnection.Send(Payload.Encode(PacketType.CommandOutput, "Jonas tinkt very hart", CryptoProvider, out _, serverType: ClientInfo.Type));
									} else {
										ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
											Info = ClientInfo,
											Socket = SocketConnection,
											Reason = status.DisconnectReason
										});
									}
									break;
								case PacketType.Ping:
									//SocketConnection.LastResponseTime = (DateTime)args.Data;
									//SocketConnection.SendSync(PacketType.Pong, null, true, args.Payload.ConversationId, ClientInfo.Type);
									break;
								default:
									DataReceived?.Invoke(this, new SocketDataReceivedEventArgs {
										Socket = SocketConnection,
										Payload = payloadObj,
										Data = payload
									});
									break;
							}
						}
					}
				};

				int connectionAttempts = 0;

				while (maxAttempts == 0 || connectionAttempts < maxAttempts) {
					try {
						connectionAttempts++;

						ConnectionWaitHandle.Reset();

						SocketConnection.ConnectAsync();

						if (!ConnectionWaitHandle.WaitOne(TimeSpan.FromSeconds(CONNECT_TIMEOUT))) {
							Log.Info($"Failed to establish connection within {CONNECT_TIMEOUT} second(s), retrying in {RECONNECT_INTERVAL} second(s). (Attempt {connectionAttempts})");
							SocketConnection.Close();
						} else {
							return;
						}

					} catch (Exception e) {
						Log.Error(e);
					}


					if (maxAttempts == 0 || connectionAttempts < MAX_RECONNECT) {
						await Task.Delay(TimeSpan.FromSeconds(RECONNECT_INTERVAL));
					} else {
						Log.Info($"Failed to establish connection within {MAX_RECONNECT} attempts(s).");
					}
				}
			}

			public void Reconnect(int maxAttempts = MAX_RECONNECT) {
				if (RemoteEndPoint == null)
					return;

				Connect(RemoteEndPoint.Address, RemoteEndPoint.Port, maxAttempts);
			}
		}
	}
}
