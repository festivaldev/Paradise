using System.Collections.Generic;
using UberStrike.Core.Models;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerOverviewState : BasePlayerState {
		public PlayerOverviewState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.ResetStatistics();
			Peer.Actor.ResetCurrentLifeStatistics();

			var players = Room.Players;

			if (players.Count > 0) {
				var allPlayers = new List<GameActorInfo>(players.Count);
				var allPositions = new List<PlayerMovement>(players.Count);

				foreach (var player in players) {
					allPlayers.Add(player.Actor.ActorInfo);
					allPositions.Add(player.Actor.Movement);
				}

				Peer.GameEventSender.SendAllPlayers(allPlayers, allPositions, 0);
			}
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
