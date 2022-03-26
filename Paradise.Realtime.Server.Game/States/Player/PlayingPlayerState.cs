using System;

namespace Paradise.Realtime.Server.Game {
	public class PlayingPlayerState : PlayerState {
		private TimeSpan DepleteTime;

		public PlayingPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			if (DateTime.UtcNow.TimeOfDay >= DepleteTime) {
				if (Peer.Actor.Info.Health > 100) {
					Peer.Actor.Info.Health--;
				}

				if (Peer.Actor.Info.ArmorPoints > 100 && Peer.Actor.Info.ArmorPoints > Peer.Actor.Info.ArmorPointCapacity) {
					Peer.Actor.Info.ArmorPoints--;
				}

				DepleteTime = DateTime.UtcNow.TimeOfDay.Add(TimeSpan.FromSeconds(1));
			}
		}
	}
}