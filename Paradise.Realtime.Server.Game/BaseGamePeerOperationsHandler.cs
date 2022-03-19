using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System;
using System.IO;

namespace Paradise.Realtime.Server.Game {
	public abstract class BaseGamePeerOperationsHandler : BaseOperationHandler<GamePeer> {
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseGamePeerOperationsHandler).Name);

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
			var operation = (IGamePeerOperationsType)(opCode);
			switch (operation) {
				case IGamePeerOperationsType.SendHeartbeatResponse:
					SendHeartbeatResponse(peer, bytes);
					break;
				case IGamePeerOperationsType.GetServerLoad:
					GetServerLoad(peer, bytes);
					break;
				case IGamePeerOperationsType.GetGameInformation:
					GetGameInformation(peer, bytes);
					break;
				case IGamePeerOperationsType.GetGameListUpdates:
					GetGameListUpdates(peer, bytes);
					break;
				case IGamePeerOperationsType.EnterRoom:
					EnterRoom(peer, bytes);
					break;
				case IGamePeerOperationsType.CreateRoom:
					CreateRoom(peer, bytes);
					break;
				case IGamePeerOperationsType.LeaveRoom:
					LeaveRoom(peer, bytes);
					break;
				case IGamePeerOperationsType.CloseRoom:
					CloseRoom(peer, bytes);
					break;
				case IGamePeerOperationsType.InspectRoom:
					InspectRoom(peer, bytes);
					break;
				case IGamePeerOperationsType.ReportPlayer:
					ReportPlayer(peer, bytes);
					break;
				case IGamePeerOperationsType.KickPlayer:
					KickPlayer(peer, bytes);
					break;
				case IGamePeerOperationsType.UpdateLoadout:
					UpdateLoadout(peer, bytes);
					break;
				case IGamePeerOperationsType.UpdatePing:
					UpdatePing(peer, bytes);
					break;
				case IGamePeerOperationsType.UpdateKeyState:
					UpdateKeyState(peer, bytes);
					break;
				case IGamePeerOperationsType.RefreshBackendData:
					RefreshBackendData(peer, bytes);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void SendHeartbeatResponse(GamePeer peer, MemoryStream bytes) {
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);
			OnSendHeartbeatResponse(peer, authToken, magicHash);
		}

		private void GetServerLoad(GamePeer peer, MemoryStream bytes) {
			OnGetServerLoad(peer);
		}

		private void GetGameInformation(GamePeer peer, MemoryStream bytes) {
			var number = Int32Proxy.Deserialize(bytes);

			OnGetGameInformation(peer, number);
		}

		private void GetGameListUpdates(GamePeer peer, MemoryStream bytes) {
			OnGetGameListUpdates(peer);
		}

		private void EnterRoom(GamePeer peer, MemoryStream bytes) {
			var roomId = Int32Proxy.Deserialize(bytes);
			var password = StringProxy.Deserialize(bytes);
			var clientVersion = StringProxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			OnEnterRoom(peer, roomId, password, clientVersion, authToken, magicHash);
		}

		private void CreateRoom(GamePeer peer, MemoryStream bytes) {
			var metaData = GameRoomDataProxy.Deserialize(bytes);
			var password = StringProxy.Deserialize(bytes);
			var clientVersion = StringProxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			OnCreateRoom(peer, metaData, password, clientVersion, authToken, magicHash);
		}

		private void LeaveRoom(GamePeer peer, MemoryStream bytes) {
			OnLeaveRoom(peer);
		}

		private void CloseRoom(GamePeer peer, MemoryStream bytes) {
			var roomId = Int32Proxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			OnCloseRoom(peer, roomId, authToken, magicHash);
		}

		private void InspectRoom(GamePeer peer, MemoryStream bytes) {
			var roomId = Int32Proxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);

			OnInspectRoom(peer, roomId, authToken);
		}

		private void ReportPlayer(GamePeer peer, MemoryStream bytes) {
			var cmid = Int32Proxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);

			OnReportPlayer(peer, cmid, authToken);
		}

		private void KickPlayer(GamePeer peer, MemoryStream bytes) {
			var cmid = Int32Proxy.Deserialize(bytes);
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			OnKickPlayer(peer, cmid, authToken, magicHash);
		}

		private void UpdateLoadout(GamePeer peer, MemoryStream bytes) {
			OnUpdateLoadout(peer);
		}

		private void UpdatePing(GamePeer peer, MemoryStream bytes) {
			var ping = UInt16Proxy.Deserialize(bytes);

			OnUpdatePing(peer, ping);
		}

		private void UpdateKeyState(GamePeer peer, MemoryStream bytes) {
			var state = ByteProxy.Deserialize(bytes);

			OnUpdateKeyState(peer, state);
		}

		private void RefreshBackendData(GamePeer peer, MemoryStream bytes) {
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			OnRefreshBackendData(peer, authToken, magicHash);
		}
	}
}
