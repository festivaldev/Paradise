using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Paradise.Realtime.Server.Game {
    public class OverviewGamePeerState : GamePeerState {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OverviewGamePeerState));

        public OverviewGamePeerState(GamePeer peer) : base(peer) {

        }

        public override void OnEnter() {
            Log.Info("entered overview state");
            var players = Room.Players;

            Log.Info($"room player count: {players.Count}");

            if (players.Count > 0) {
                var allPlayers = new List<GameActorInfo>(players.Count);
                var allPositions = new List<PlayerMovement>(players.Count);

                foreach (var player in players) {
                    allPlayers.Add(player.Actor.Info);
                    allPositions.Add(player.Actor.Movement);

                    Debug.Assert(player.Actor.Info.PlayerId == player.Actor.Movement.Number);

                    Peer.KnownActors.Add(player.Actor.Cmid);
                }

                Peer.Events.Room.SendAllPlayers(allPlayers, allPositions, 0);
            }
        }

        public override void OnResume() { }

        public override void OnExit() { }

        public override void OnUpdate() { }
    }
}
