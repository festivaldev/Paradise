using UberStrike.Core.Models;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerPrepareState : BasePlayerState {
		public PlayerPrepareState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.ActorInfo.PlayerState = PlayerStates.None;

			Peer.GameEventSender.SendPrepareNextRound();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
