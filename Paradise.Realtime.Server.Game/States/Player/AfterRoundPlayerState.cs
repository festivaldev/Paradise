using Paradise.Core.Models;
using System;

namespace Paradise.Realtime.Server.Game {
	public class AfterRoundPlayerState : PlayerState {
		public AfterRoundPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.GameEvents.SendMatchEnd(new EndOfMatchData {
			
			});
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}