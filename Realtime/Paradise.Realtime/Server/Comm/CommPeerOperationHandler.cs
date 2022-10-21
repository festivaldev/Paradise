using log4net;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeerOperationHandler : BaseCommPeerOperationHandler {
		private static readonly ILog Log = LogManager.GetLogger("CommLog");

		private static UserWebServiceClient ServiceClient;

		public CommPeerOperationHandler() : base() {
			if (ServiceClient == null) {
				ServiceClient = new UserWebServiceClient(
					endpointUrl: CommApplication.Instance.Configuration.WebServiceBaseUrl,
					webServicePrefix: CommApplication.Instance.Configuration.WebServicePrefix,
					webServiceSuffix: CommApplication.Instance.Configuration.WebServiceSuffix
				);
			}
		}

		public override void OnAuthenticationRequest(CommPeer peer, string authToken, string magicHash) {
			peer.Authenticate(authToken, magicHash);

			// Retrieve user data from the web server.
			var member = ServiceClient.GetMember(authToken);

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

		public override void OnSendHeartbeatResponse(CommPeer peer, string authToken, string responseHash) {
			try {
				if (!peer.CheckHeartbeat(responseHash)) {
					peer.SendError();
				}
			} catch (Exception e) {
				Log.Error("Exception while checking heartbeat", e);
				peer.SendError();
			}
		}
	}
}
