using System;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerPlayingState : BasePlayerState {
		private TimeSpan DepleteTime;

		public PlayerPlayingState(GamePeer peer) : base(peer) { }

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			if (DateTime.UtcNow.TimeOfDay >= DepleteTime) {
				if (Peer.Actor.ActorInfo.Health > 100) {
					Peer.Actor.ActorInfo.Health--;
				}

				if (Peer.Actor.ActorInfo.ArmorPoints > Peer.Actor.ActorInfo.ArmorPointCapacity) {
					Peer.Actor.ActorInfo.ArmorPoints--;
				}

				DepleteTime = DateTime.UtcNow.TimeOfDay.Add(TimeSpan.FromSeconds(1));
			}
		}
	}
}
