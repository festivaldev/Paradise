using Paradise.Core.Models;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Photon.SocketServer;
using System;
using System.Diagnostics;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeer : Peer {
		public LobbyRoom Room { get; set; }
		public CommActor Actor { get; set; }
		public CommPeerEvents Events { get; }

		public CommPeer(InitRequest request) : base(request) {
			Events = new CommPeerEvents(this);
			Handlers.Add(new CommPeerOperationHandler());
		}

		public override void SendHeartbeat(string hash) {
			Events.SendHeartbeatChallenge(hash);
		}

		public override void SendError(string message = "An error occured that forced UberStrike to halt.") {
			var trace = new StackTrace();
			Log.Info($"sending error to peer: {message} {trace}");
			base.SendError(message);
			Events.SendDisconnectAndDisablePhoton(message);
		}

		public override void Tick() {
			base.Tick();

			if (Actor.IsMuted && DateTime.UtcNow >= Actor.MuteEndTime)
				Actor.IsMuted = false;
		}

		protected override void OnAuthenticate(UberstrikeUserViewModel userView) {
			var actorView = new CommActorInfo {
				AccessLevel = userView.CmuneMemberView.PublicProfile.AccessLevel,
				Channel = ChannelType.Steam,
				Cmid = userView.CmuneMemberView.PublicProfile.Cmid,
				PlayerName = userView.CmuneMemberView.PublicProfile.Name,
			};

			Actor = new CommActor(this, actorView);
		}
	}
}
