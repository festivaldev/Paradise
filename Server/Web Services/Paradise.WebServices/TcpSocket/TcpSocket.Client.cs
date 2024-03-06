using log4net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Paradise {
	public partial class TcpSocket {
		public class SocketClient {
			readonly byte[] MAGIC_BYTES = new byte[] { 0x65, 0x53, 0x69, 0x44, 0x61, 0x52, 0x61, 0x50 /* 0x50 */ };

			protected static readonly ILog Log = LogManager.GetLogger(nameof(SocketClient));

			private readonly SocketConnection SocketConnection;

			private SocketInfo ClientInfo;
			private readonly RijndaelManaged CryptoProvider;

			public event EventHandler<SocketConnectedEventArgs> Connected;
			public event EventHandler<SocketDisconnectedEventArgs> Disconnected;
			public event EventHandler<SocketDataReceivedEventArgs> DataReceived;
			public event EventHandler<SocketConnectionRejectedEventArgs> ConnectionRejected;

			public SocketClient(string serverId, ServerType serverType, string encryptionPassPhrase) {
				try {
					ClientInfo = new SocketInfo {
						SocketId = Guid.Parse(serverId),
						Type = serverType
					};

					CryptoProvider = new RijndaelManaged {
						KeySize = 256,
						Key = new Rfc2898DeriveBytes(encryptionPassPhrase, ClientInfo.SocketId.ToByteArray()).GetBytes(32),
						IV = ClientInfo.SocketId.ToByteArray()
					};

					SocketConnection = new SocketConnection {
						CryptoProvider = CryptoProvider
					};

					SocketConnection.Connected += OnConnected;
					SocketConnection.Disconnected += (object sender, SocketDisconnectedEventArgs args) => {
						Disconnected?.Invoke(this, args);
					};
					SocketConnection.DataReceived += (object sender, SocketDataReceivedEventArgs args) => {
						switch (args.Type) {
							case PacketType.Ping:
								SocketConnection.LastResponseTime = (DateTime)args.Data;
								SocketConnection.SendSync(PacketType.Pong, null, true, args.Payload.ConversationId, ClientInfo.Type);
								break;
							default:
								DataReceived?.Invoke(this, args);
								break;
						}
					};
				} catch (Exception e) {
					Log.Error(e);
				}
			}

			public async Task<bool> Connect(IPAddress ip, int port) {
				return await SocketConnection.ConnectToServer(ip, port);
			}

			public void Reconnect(int maxAttempts = SocketConnection.MAX_RECONNECT) {
				SocketConnection.ReconnectToServer(maxAttempts);
			}

			#region Send
			public async Task SendBytes(byte[] bytes) {
				await SocketConnection.SendBytes(bytes);
			}

			public void SendBytesSync(byte[] bytes) {
				SocketConnection.SendBytesSync(bytes);
			}

			public async Task SendPacket(PacketType type) {
				await SocketConnection.SendPacket(type);
			}

			public void SendPacketSync(PacketType type) {
				SocketConnection.SendPacketSync(type);
			}

			public async Task<object> Send(PacketType type, object payload, bool oneWay = true, Guid conversationId = default, ServerType serverType = ServerType.None) {
				return await SocketConnection.Send(type, payload, oneWay, conversationId, serverType);
			}

			public object SendSync(PacketType type, object payload, bool oneWay = true, Guid conversationId = default, ServerType serverType = ServerType.None) {
				return SocketConnection.Send(type, payload, oneWay, conversationId, serverType);
			}
			#endregion

			#region Callbacks
			private async void OnConnected(object sender, SocketConnectedEventArgs args) {
				//Console.WriteLine("connected");

				try {
					if (SocketConnection.ReadBytes(out var magicByteRequest) > 0 && (PacketType)BitConverter.ToInt32(magicByteRequest, 0) == PacketType.MagicBytes) {
						await SocketConnection.SendBytes(MAGIC_BYTES);
					} else {
						Console.WriteLine(BitConverter.ToInt32(magicByteRequest, 0));
						Console.WriteLine(BitConverter.ToString(magicByteRequest).Replace('-', ' '));
						Console.WriteLine("Client disconnect");
						SocketConnection.Disconnect();
						return;
					}

					if (SocketConnection.ReadBytes(out var clientInfoRequest) > 0 && (PacketType)BitConverter.ToInt32(clientInfoRequest, 0) == PacketType.ClientInfo) {
						await SocketConnection.Send(PacketType.ClientInfo, ClientInfo, true, default, ClientInfo.Type);
					} else {
						SocketConnection.Disconnect();
						return;
					}

					var connectionStatus = SocketConnection.Read<SocketConnectionStatus>(out var payloadObj);
					//Console.WriteLine($"connected: {connectionStatus.Connected}, rejected: {connectionStatus.Rejected}, reason: {connectionStatus.DisconnectReason}");
					if (connectionStatus.Connected) {
						//Console.WriteLine("connection accepted");
						Connected?.Invoke(this, new SocketConnectedEventArgs {
							Socket = SocketConnection
						});

						SocketConnection.BeginReceivingData();
					} else {
						ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
							Info = ClientInfo,
							Socket = SocketConnection,
							Reason = connectionStatus.DisconnectReason
						});
					}
				} catch (SocketException) {
					//Console.WriteLine("server forced disconnect");
					SocketConnection.Disconnect();
				} catch (Exception e) {
					Log.Error(e);
				}
			}
			#endregion
		}
	}
}
