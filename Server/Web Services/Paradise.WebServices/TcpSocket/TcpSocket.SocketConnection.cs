using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paradise {
	public partial class TcpSocket {
		public class SocketConnection {
			public const int CONNECT_TIMEOUT = 5;
			public const int RECONNECT_INTERVAL = 10;
			public const int MAX_RECONNECT = 10;
			public const int SEND_TIMEOUT = 3;
			public const int RECEIVE_TIMEOUT = 3;

			protected static readonly ILog Log = LogManager.GetLogger(nameof(SocketConnection));

			public Guid ConnectionId;
			public Socket Socket;
			public SocketInfo Info;
			public RijndaelManaged CryptoProvider;
			public byte[] MessageBuffer;
			public string DisconnectReason;
			public DateTime LastResponseTime;

			protected System.Timers.Timer PingTimer;

			private SocketState _connectionState = SocketState.Disconnected;
			public SocketState ConnectionState {
				get {
					return _connectionState;
				}

				private set {
					_connectionState = value;

					StateChanged?.Invoke(this, new SocketStateChangedEventArgs {
						Socket = this,
						State = value
					});
				}
			}

			public Guid Identifier {
				get {
					return Info.SocketId;
				}
			}

			public ServerType Type {
				get {
					return Info.Type;
				}
			}

			public bool IsConnected {
				get {
					try {
						return !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
					} catch (ObjectDisposedException) {
						return false;
					} catch (SocketException) {
						return false;
					}
				}
			}

			private IPEndPoint RemoteEndPoint;
			private readonly ManualResetEvent ConnectionWaitHandle = new ManualResetEvent(false);

			private Task<int> sendTask;
			private Task<object> receiveTask;
			private readonly Dictionary<Guid, TaskCompletionSource<object>> receiveTasks = new Dictionary<Guid, TaskCompletionSource<object>>();

			public event EventHandler<SocketStateChangedEventArgs> StateChanged;
			public event EventHandler<SocketConnectedEventArgs> Connected;
			public event EventHandler<SocketDisconnectedEventArgs> Disconnected;
			public event EventHandler<SocketDataReceivedEventArgs> DataReceived;

			public async Task<bool> ConnectToServer(IPAddress ip, int port, int maxAttempts = MAX_RECONNECT) {
				RemoteEndPoint = new IPEndPoint(ip, port);

				int connectionAttempts = 0;

				while (maxAttempts == 0 || connectionAttempts < maxAttempts) {
					try {
						connectionAttempts++;

						ConnectionWaitHandle.Reset();

						Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						var result = Socket.BeginConnect(RemoteEndPoint, new AsyncCallback(OnConnectedToServer), null);

						ConnectionState = SocketState.Connecting;

						if (!ConnectionWaitHandle.WaitOne(TimeSpan.FromSeconds(CONNECT_TIMEOUT))) {
							Log.Error($"Failed to establish connection within {CONNECT_TIMEOUT} second(s), retrying in {RECONNECT_INTERVAL} second(s). (Attempt {connectionAttempts})");
							Socket.Close();
						} else {
							return true;
						}

					} catch (Exception e) {
						Log.Error(e);
					}


					if (maxAttempts == 0 || connectionAttempts < MAX_RECONNECT) {
						await Task.Delay(TimeSpan.FromSeconds(RECONNECT_INTERVAL));
					} else {
						Log.Error($"Failed to establish connection within {MAX_RECONNECT} attempts(s).");
					}
				}

				ConnectionState = SocketState.Disconnected;

				return false;
			}

			public void ReconnectToServer(int maxAttempts = MAX_RECONNECT) {
				if (RemoteEndPoint == null) return;

				Task.Run(async () => {
					await ConnectToServer(RemoteEndPoint.Address, RemoteEndPoint.Port, maxAttempts);
				});
			}

			public void Disconnect() {
				//if (!Socket.Connected) return;

				try {
					ConnectionState = SocketState.Disconnecting;
					Socket.BeginDisconnect(false, new AsyncCallback(OnDisconnected), null);
				} catch (ObjectDisposedException) {

				}
			}

			public void BeginReceivingData() {
				Socket.BeginReceive(MessageBuffer, 0, Socket.ReceiveBufferSize, 0, new AsyncCallback(OnDataReceived), null);
			}

			#region Callbacks
			private void OnConnectedToServer(IAsyncResult result) {
				try {
					if (!Socket.Connected) return;
					Socket.EndConnect(result);

					MessageBuffer = new byte[Socket.ReceiveBufferSize];

					ConnectionWaitHandle.Set();
					ConnectionState = SocketState.Connected;

					Connected?.Invoke(this, new SocketConnectedEventArgs {
						Socket = this
					});
				} catch (Exception e) {
					Log.Error(e);
				}
			}

			private void OnDisconnected(IAsyncResult result) {
				try {
					Socket.EndDisconnect(result);

					Socket.Close(0);

					ConnectionState = SocketState.Disconnected;
					Disconnected?.Invoke(this, new SocketDisconnectedEventArgs {
						Socket = this,
						Info = Info,
						Reason = DisconnectReason
					});
				} catch { }
			}

			private void OnDataReceived(IAsyncResult result) {
				if (DataReceived == null) return;

				var stringBuilder = new StringBuilder();

				try {
					int totalBytesRead = 0;
					var bytesRead = Socket.EndReceive(result);

					while (bytesRead > 0) {
						totalBytesRead += bytesRead;

						stringBuilder.Append(Encoding.UTF8.GetString(MessageBuffer, 0, bytesRead));

						if (!Socket.Poll(0, SelectMode.SelectRead)) break;

						bytesRead = Socket.Receive(MessageBuffer, 0, Socket.ReceiveBufferSize, 0);
					}

					if (totalBytesRead > 0) {
						var base64 = stringBuilder.ToString();

						foreach (var _base64 in base64.Split('|')) {
							var data = Payload.Decode<object>(_base64, CryptoProvider, out var payloadObj);

							DataReceived?.Invoke(this, new SocketDataReceivedEventArgs {
								Socket = this,
								Payload = payloadObj,
								Data = data
							});

							if (receiveTasks.ContainsKey(payloadObj.ConversationId)) {
								receiveTasks[payloadObj.ConversationId].SetResult(data);
								receiveTasks.Remove(payloadObj.ConversationId);
							}
						}
					}

					if (IsConnected) {
						Socket.BeginReceive(MessageBuffer, 0, Socket.ReceiveBufferSize, 0, new AsyncCallback(OnDataReceived), null);
					}
				} catch (ObjectDisposedException) {
					// Socket disconnected, object disposed
				} catch (SocketException) {
					// Socket closed connection

					DisconnectReason = "Connection closed by client";
					Disconnect();
				} catch (Exception e) {
					Log.Error(e);
				}
			}
			#endregion

			#region Send
			public async Task SendBytes(byte[] bytes) {
				if (!IsConnected) throw new SocketException();

				if (sendTask != null) {
					//Console.WriteLine("socket is currently sending data");
					await sendTask;
				}

				if (receiveTask != null) {
					//Console.WriteLine("socket is currently receiving data");
					await receiveTask;
				}

				try {
					//Console.WriteLine($"Sending {bytes.Length} bytes");
					ConnectionState = SocketState.Sending;

					sendTask = Socket.SendAsync(new ArraySegment<byte>(bytes), 0);
					var timeout = Task.Delay(TimeSpan.FromSeconds(SEND_TIMEOUT));

					//Console.WriteLine("awaiting send or timeout");
					if (await Task.WhenAny(sendTask, timeout) == timeout) {
						Log.Error($"Failed to send data within {SEND_TIMEOUT} second(s).");
						sendTask = null;
						return;
					}

					await sendTask;
					//Console.WriteLine("awaited send task");
					sendTask = null;
					ConnectionState = SocketState.Connected;
				} catch {
					return;
				}
			}

			public void SendBytesSync(byte[] bytes) {
				Task.Run(async () => {
					await SendBytes(bytes);
				});
			}

			public async Task SendPacket(PacketType type) {
				await SendBytes(BitConverter.GetBytes(Convert.ToInt32(type)));
			}

			public void SendPacketSync(PacketType type) {
				SendBytesSync(BitConverter.GetBytes(Convert.ToInt32(type)));
			}

			public async Task<object> Send(PacketType type, object payload, bool oneWay = true, Guid conversationId = default, ServerType serverType = ServerType.None) {
				await SendBytes(Payload.Encode(type, payload, CryptoProvider, out var payloadObj, oneWay, conversationId, serverType));

				if (oneWay) return null;

				//Log.Info("awaiting response");
				ConnectionState = SocketState.Receiving;

				receiveTasks[payloadObj.ConversationId] = new TaskCompletionSource<object>();
				receiveTask = receiveTasks[payloadObj.ConversationId].Task;

				var timeout = Task.Delay(TimeSpan.FromSeconds(RECEIVE_TIMEOUT));

				//Console.WriteLine("awaiting receive or timeout");
				if (await Task.WhenAny(receiveTask, timeout) == timeout) {
					Log.Error($"Failed to receive data within {RECEIVE_TIMEOUT} second(s).");
					receiveTask = null;
					return null;
				}

				var taskData = await receiveTask;
				//Console.WriteLine("awaited response");
				receiveTask = null;
				ConnectionState = SocketState.Connected;

				return taskData;
			}

			public object SendSync(PacketType type, object payload, bool oneWay = true, Guid conversationId = default, ServerType serverType = ServerType.None) {
				return Task.Run(async () => {
					return await Send(type, payload, oneWay, conversationId, serverType);
				}).Result;
			}
			#endregion

			#region Receive
			public int ReadBytes(out byte[] outBuffer, int length = 0) {
				outBuffer = new byte[] { };

				if (!IsConnected) throw new SocketException();

				var buffer = new byte[Socket.ReceiveBufferSize];

				try {
					var bytesRead = Socket.Receive(buffer, 0, (length > 0 ? length : Socket.ReceiveBufferSize), 0);

					while (bytesRead > 0) {
						Array.Resize(ref outBuffer, outBuffer.Length + bytesRead);
						Array.Copy(buffer, 0, outBuffer, 0, bytesRead);

						if (!Socket.Poll(0, SelectMode.SelectRead) || bytesRead == length) break;
						bytesRead = Socket.Receive(buffer, 0, Socket.ReceiveBufferSize, 0);
					}

					//Console.WriteLine($"read {outBuffer.Length} bytes");

					return bytesRead;
				} catch (SocketException) {

				} catch (Exception e) {
					Log.Error(e);
				}

				return 0;
			}

			public T Read<T>(out Payload payloadObj) {
				ReadBytes(out var bytes);

				return Payload.Decode<T>(Encoding.UTF8.GetString(bytes), CryptoProvider, out payloadObj);
			}
			#endregion
		}
	}
}
