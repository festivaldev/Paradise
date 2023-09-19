using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public partial class CommPeer {
		public class OperationHandler : BaseOperationHandler<CommPeer, ICommPeerOperationsType> {
			private static readonly new ILog Log = LogManager.GetLogger(nameof(CommPeer.OperationHandler));

			public override int Id => (int)OperationHandlerId.CommPeer;

			public override void OnOperationRequest(CommPeer peer, byte opCode, MemoryStream bytes) {
				switch ((ICommPeerOperationsType)opCode) {
					case ICommPeerOperationsType.SendHeartbeatResponse:
						break;
					default:
						Log.Debug($"CommPeer.OperationHandler::OnOperationRequest -> peer: {peer}, opCode: {(ICommPeerOperationsType)opCode}({opCode})");
						break;
				}

				switch ((ICommPeerOperationsType)opCode) {
					case ICommPeerOperationsType.AuthenticationRequest:
						AuthenticateRequest(peer, bytes);
						break;

					case ICommPeerOperationsType.SendHeartbeatResponse:
						SendHeartbeatResponse(peer, bytes);
						break;

					default:
						throw new NotSupportedException();
				}
			}

			#region Implementation of ICommPeerOperationsType
			private void AuthenticateRequest(CommPeer peer, MemoryStream bytes) {
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, authToken, magicHash);

				if (!peer.Authenticate(authToken, magicHash)) {
					peer.SendError("Failed to authenticate. Please try again later.\n(Magic Hash verification failure)");
					return;
				}

				if (!(UserWebServiceClient.Instance.GetMember(authToken) is var member) || member.CmuneMemberView == null) {
					peer.SendError("Failed to authenticate. Please try again later.\n(Member was null while authenticating)");
					return;
				}

				peer.Member = member;

				var actor = new CommActor(peer, new CommActorInfo {
					AccessLevel = member.CmuneMemberView.PublicProfile.AccessLevel,
					Channel = ChannelType.Steam,
					Cmid = member.CmuneMemberView.PublicProfile.Cmid,
					PlayerName = member.CmuneMemberView.PublicProfile.Name,
				});

				peer.Actor = actor;

				LobbyManager.Instance.GlobalLobby.Join(peer);
				LobbyManager.Instance.UpdatePlayerList();
			}

			private void SendHeartbeatResponse(CommPeer peer, MemoryStream bytes) {
				var authToken = StringProxy.Deserialize(bytes);
				var responseHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, authToken, responseHash);

				try {
					if (!peer.CheckHeartbeat(responseHash)) {
						peer.SendError("An error occured that forced UberStrike to halt.\n(Heartbeat verification failure)");
					}
				} catch (Exception e) {
					Log.Error("Exception while checking heartbeat", e);
					peer.SendError();
				}
			}
			#endregion



			private void DebugOperation(params object[] data) {
#if DEBUG
				Log.Debug($"{GetType().Name}:{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
			}
		}
	}
}
