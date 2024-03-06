using log4net;
using Paradise.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Paradise {
	public partial class TcpSocket {
		public class SocketServer {
			const int CLIENT_PING_INTERVAL = 10;
			readonly byte[] MAGIC_BYTES = new byte[] { 0x50, 0x61, 0x52, 0x61, 0x44, 0x69, 0x53, 0x65 };

			protected static readonly ILog Log = LogManager.GetLogger(nameof(SocketServer));

			private readonly IPEndPoint LocalEndPoint;
			private Socket SocketListener;

			private SocketConnection CommServer;
			private readonly List<SocketConnection> GameServers = new List<SocketConnection>();

			private readonly Dictionary<Guid, SocketConnection> ConnectedSockets = new Dictionary<Guid, SocketConnection>();
			private readonly Dictionary<Guid, RijndaelManaged> CryptoProviders = new Dictionary<Guid, RijndaelManaged>();

			private readonly object _lock = new object();

			public event EventHandler<SocketConnectedEventArgs> ClientConnected;
			public event EventHandler<SocketDisconnectedEventArgs> ClientDisconnected;
			public event EventHandler<SocketDataReceivedEventArgs> DataReceived;
			public event EventHandler<SocketConnectionRejectedEventArgs> ConnectionRejected;

			public SocketServer(int port) {
				LocalEndPoint = new IPEndPoint(IPAddress.Any, port);
			}

			public void Listen() {
				if (SocketListener != null)
					throw new InvalidOperationException("Socket is already listening");

				SocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				SocketListener.Bind(LocalEndPoint);
				SocketListener.Listen(10);

				Log.Info($"Socket server listening on {LocalEndPoint}");

				SocketListener.BeginAccept(new AsyncCallback(OnClientAccepted), null);
			}

			public void Shutdown() {
				if (SocketListener == null)
					return;

				SocketListener.Close(0);
			}

			#region Send
			public async Task<object> SendToCommServer(PacketType type, object payload, bool isOneWay = true, Guid conversationId = default) {
				if (CommServer == null)
					return null;

				var r = await CommServer.Send(type, payload, isOneWay, conversationId, ServerType.MasterServer);
				return r;
			}

			public void SendToGameServers(PacketType type, object payload) {
				// Sending to game servers is one-way only

				foreach (var server in GameServers) {
					server.SendSync(type, payload, true);
				}
			}
			#endregion

			#region Callbacks
			public async void OnClientAccepted(IAsyncResult result) {
				try {
					var client = SocketListener.EndAccept(result);
					SocketListener.BeginAccept(new AsyncCallback(OnClientAccepted), null);

					//Console.WriteLine("accepted client");

					var socketClient = new SocketConnection {
						ConnectionId = Guid.NewGuid(),
						Socket = client,
						MessageBuffer = new byte[client.ReceiveBufferSize],
						Info = new SocketInfo {
							IsClient = true
						}
					};

					await socketClient.SendPacket(PacketType.MagicBytes);

					if (socketClient.ReadBytes(out var magicBytes) <= 0 || !magicBytes.Reverse().ToArray().SequenceEqual(MAGIC_BYTES)) {
						socketClient.Disconnect();
						return;
					}

					await socketClient.SendPacket(PacketType.ClientInfo);

					var clientInfo = socketClient.Read<SocketInfo>(out var payloadObj);

					socketClient.Info.SocketId = clientInfo.SocketId;
					socketClient.Info.Type = clientInfo.Type;

					if (ParadiseService.WebServiceSettings.ServerPassPhrases.Find(_ => _.Id == socketClient.Identifier)?.PassPhrase.Trim() is var passPhrase && string.IsNullOrWhiteSpace(passPhrase)) {
						socketClient.DisconnectReason = "Unknown server";

						ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
							Info = clientInfo,
							Socket = socketClient,
							Reason = socketClient.DisconnectReason
						});

						await socketClient.Send(PacketType.ConnectionStatus, new SocketConnectionStatus {
							Connected = false,
							Rejected = true,
							DisconnectReason = socketClient.DisconnectReason
						}, true, payloadObj.ConversationId);

						socketClient.Disconnect();

						return;
					}

					switch (clientInfo.Type) {
						case ServerType.Comm:
							if (CommServer != null) {
								socketClient.DisconnectReason = "Cannot register more than one Comm Server";

								ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
									Info = clientInfo,
									Socket = socketClient,
									Reason = socketClient.DisconnectReason
								});

								await socketClient.Send(PacketType.ConnectionStatus, new SocketConnectionStatus {
									Connected = false,
									Rejected = true,
									DisconnectReason = socketClient.DisconnectReason
								}, true, payloadObj.ConversationId);

								socketClient.Disconnect();

								return;
							}

							CommServer = socketClient;

							break;

						case ServerType.Game:
							if (GameServers.Where(_ => _.Identifier == socketClient.Identifier).FirstOrDefault() != null) {
								socketClient.DisconnectReason = "Duplicate server identifier";

								ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
									Info = clientInfo,
									Socket = socketClient,
									Reason = socketClient.DisconnectReason
								});

								await socketClient.Send(PacketType.ConnectionStatus, new SocketConnectionStatus {
									Connected = false,
									Rejected = true,
									DisconnectReason = socketClient.DisconnectReason
								}, true, payloadObj.ConversationId);

								socketClient.Disconnect();

								return;
							}

							GameServers.Add(socketClient);

							break;

						default:
							socketClient.DisconnectReason = "Unknown server type";

							ConnectionRejected?.Invoke(this, new SocketConnectionRejectedEventArgs {
								Info = clientInfo,
								Socket = socketClient,
								Reason = socketClient.DisconnectReason
							});

							await socketClient.Send(PacketType.ConnectionStatus, new SocketConnectionStatus {
								Connected = false,
								Rejected = true,
								DisconnectReason = socketClient.DisconnectReason
							}, true, payloadObj.ConversationId);

							socketClient.Disconnect();

							return;
					}

					lock (_lock) {
						ConnectedSockets.Add(socketClient.ConnectionId, socketClient);

						CryptoProviders.Add(socketClient.ConnectionId, new RijndaelManaged {
							KeySize = 256,
							Key = new Rfc2898DeriveBytes(passPhrase, clientInfo.SocketId.ToByteArray()).GetBytes(32),
							IV = clientInfo.SocketId.ToByteArray()
						});

						socketClient.CryptoProvider = CryptoProviders[socketClient.ConnectionId];
					}

					ClientConnected?.Invoke(this, new SocketConnectedEventArgs {
						Socket = socketClient
					});

					await socketClient.Send(PacketType.ConnectionStatus, new SocketConnectionStatus {
						Connected = true
					}, true, payloadObj.ConversationId);

					socketClient.DataReceived += OnDataReceived;
					socketClient.Disconnected += OnClientDisconnected;

					socketClient.BeginReceivingData();
				} catch (SocketException) {
					Log.Debug("Socket disconnected");
				} catch (ObjectDisposedException) {

				} catch (Exception e) {
					Log.Error(e);
				}
			}

			private void OnClientDisconnected(object sender, SocketDisconnectedEventArgs args) {
				//Console.WriteLine($"client {args.Socket.Identifier} disconnected");
				var socketClient = args.Socket;
				var client = socketClient.Socket;

				try {
					if (ConnectedSockets.ContainsValue(socketClient)) {
						lock (_lock) {
							ConnectedSockets.Remove(socketClient.ConnectionId);

							switch (socketClient.Type) {
								case ServerType.Comm:
									if (CommServer.ConnectionId == socketClient.ConnectionId) {
										CommServer = null;
									}
									break;

								case ServerType.Game:
									if (GameServers.Contains(socketClient)) {
										GameServers.Remove(socketClient);
									}
									break;

								default:
									break;
							}

							if (CryptoProviders.ContainsKey(socketClient.ConnectionId)) {
								CryptoProviders.Remove(socketClient.ConnectionId);
							}
						}

						ClientDisconnected?.Invoke(this, new SocketDisconnectedEventArgs {
							Info = socketClient.Info,
							Socket = socketClient,
							Reason = socketClient.DisconnectReason
						});
					}

					client.Close();
				} catch (NullReferenceException e) {
					Log.Error(e);
				} catch (Exception e) {
					Log.Error(e);
				}
			}

			private void OnDataReceived(object sender, SocketDataReceivedEventArgs args) {
				switch (args.Type) {
					case PacketType.Pong:
						//Log.Info($"got pong {args.Data}");
						args.Socket.LastResponseTime = (DateTime)args.Data;
						break;
					default:
						DataReceived?.Invoke(this, args);
						break;
				}
			}
			#endregion
		}
	}
}
