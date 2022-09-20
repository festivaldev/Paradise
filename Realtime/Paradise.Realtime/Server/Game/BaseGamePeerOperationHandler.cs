using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System;
using System.IO;

namespace Paradise.Realtime.Server.Game {
	public abstract class BaseGamePeerOperationHandler : BaseOperationHandler<GamePeer> {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BaseGamePeerOperationHandler));

		public override int Id => 1;

		protected abstract void OnSendHeartbeatResponse(GamePeer peer, string authToken, string responseHash);
		protected abstract void OnGetServerLoad(GamePeer peer);
		protected abstract void OnGetGameInformation(GamePeer peer, int number);
		protected abstract void OnGetGameListUpdates(GamePeer peer);
		protected abstract void OnEnterRoom(GamePeer peer, int roomId, string password, string clientVersion, string authToken, string magicHash);
		protected abstract void OnCreateRoom(GamePeer peer, GameRoomData metaData, string password, string clientVersion, string authToken, string magicHash);
		protected abstract void OnLeaveRoom(GamePeer peer);
		protected abstract void OnCloseRoom(GamePeer peer, int roomId, string authToken, string magicHash);
		protected abstract void OnInspectRoom(GamePeer peer, int roomId, string authToken);
		protected abstract void OnReportPlayer(GamePeer peer, int cmid, string authToken);
		protected abstract void OnKickPlayer(GamePeer peer, int cmid, string authToken, string magicHash);
		protected abstract void OnUpdateLoadout(GamePeer peer);
		protected abstract void OnUpdatePing(GamePeer peer, ushort ping);
		protected abstract void OnUpdateKeyState(GamePeer peer, byte state);
		protected abstract void OnRefreshBackendData(GamePeer peer, string authToken, string magicHash);

		public override void OnOperationRequest(GamePeer peer, byte opCode, MemoryStream bytes) {
			var operation = (IGamePeerOperationsType)opCode;

			switch (operation) {
				case IGamePeerOperationsType.SendHeartbeatResponse:
				case IGamePeerOperationsType.UpdatePing:
				case IGamePeerOperationsType.UpdateKeyState:
					break;
				default:
					Log.Debug($"GamePeer.OnOperationRequest peer: {peer}, opCode: {operation}({opCode})");
					break;
			}

			switch (operation) {
				case IGamePeerOperationsType.SendHeartbeatResponse: {
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnSendHeartbeatResponse(peer, authToken, magicHash);
					break;
				}
				case IGamePeerOperationsType.GetServerLoad: {
					OnGetServerLoad(peer);
					break;
				}
				case IGamePeerOperationsType.GetGameInformation: {
					var number = Int32Proxy.Deserialize(bytes);

					OnGetGameInformation(peer, number);
					break;
				}
				case IGamePeerOperationsType.GetGameListUpdates: {
					OnGetGameListUpdates(peer);
					break;
				}
				case IGamePeerOperationsType.EnterRoom: {
					var roomId = Int32Proxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var clientVersion = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnEnterRoom(peer, roomId, password, clientVersion, authToken, magicHash);
					break;
				}
				case IGamePeerOperationsType.CreateRoom: {
					var metaData = GameRoomDataProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var clientVersion = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnCreateRoom(peer, metaData, password, clientVersion, authToken, magicHash);
					break;
				}
				case IGamePeerOperationsType.LeaveRoom: {
					OnLeaveRoom(peer);
					break;
				}
				case IGamePeerOperationsType.CloseRoom: {
					var roomId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnCloseRoom(peer, roomId, authToken, magicHash);
					break;
				}
				case IGamePeerOperationsType.InspectRoom: {
					var roomId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					OnInspectRoom(peer, roomId, authToken);
					break;
				}
				case IGamePeerOperationsType.ReportPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					OnReportPlayer(peer, cmid, authToken);
					break;
				}
				case IGamePeerOperationsType.KickPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnKickPlayer(peer, cmid, authToken, magicHash);
					break;
				}
				case IGamePeerOperationsType.UpdateLoadout: {
					OnUpdateLoadout(peer);
					break;
				}
				case IGamePeerOperationsType.UpdatePing: {
					var ping = UInt16Proxy.Deserialize(bytes);

					OnUpdatePing(peer, ping);
					break;
				}
				case IGamePeerOperationsType.UpdateKeyState: {
					var state = ByteProxy.Deserialize(bytes);

					OnUpdateKeyState(peer, state);
					break;
				}
				case IGamePeerOperationsType.RefreshBackendData: {
					var authToken = StringProxy.Deserialize(bytes);
					var magicHash = StringProxy.Deserialize(bytes);

					OnRefreshBackendData(peer, authToken, magicHash);
					break;
				}
				default:
					throw new NotSupportedException();
			}
		}
	}
}
