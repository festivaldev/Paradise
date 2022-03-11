using log4net;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Client;
using System;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeerOperationHandler : BaseCommPeerOperationHandler {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CommPeerOperationHandler));

		public CommPeerOperationHandler() { }

		public override void OnAuthenticationRequest(CommPeer peer, string authToken, string magicHash) {
			peer.Authenticate(authToken, magicHash);

			// Retrieve user data from the web server.
			var client = new UserWebServiceClient(CommApplication.Instance.Configuration.WebServiceBaseUrl);
			var member = client.GetMember(authToken);

			var actor = new CommActor(peer, new CommActorInfo {
				AccessLevel = member.CmuneMemberView.PublicProfile.AccessLevel,
				Channel = ChannelType.Steam,
				Cmid = member.CmuneMemberView.PublicProfile.Cmid,
				PlayerName = member.CmuneMemberView.PublicProfile.Name,
			});

			peer.Actor = actor;

			LobbyManager.Instance.Join(peer);
			LobbyManager.Instance.UpdatePlayerList();
		}

		public override void OnSendHeartbeatResponse(CommPeer peer, string authToken, string responseHash) {
			try {
				if (false) {
					peer.SendError();
				}
			} catch (Exception e) {
				Log.Error("Exception while checking heartbeat", e);
				peer.SendError();
			}
		}
	}
}
