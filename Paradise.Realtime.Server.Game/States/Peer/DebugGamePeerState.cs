using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class DebugGamePeerState : GamePeerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugGamePeerState));

		public DebugGamePeerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			var players = Room.Players;

            /* Let the client know about the other players in the room, if there is any. */
            if (players.Count > 0)
            {
                var allPlayers = new List<GameActorInfo>(players.Count);
                var allPositions = new List<PlayerMovement>(players.Count);
                foreach (var player in players)
                {
                    
                    allPlayers.Add(player.Info);
                    allPositions.Add(player.Movement);

                    //Debug.Assert(player.Actor.Info.PlayerId == player.Actor.Movement.Number);

                    //Peer.KnownActors.Add(player.Actor.Cmid);
                }

                Peer.Events.Game.SendAllPlayers(allPlayers, allPositions, 0);
            }
		}

		public override void OnResume() { }

		public override void OnExit() { }

		public override void OnUpdate() { }
	}
}
