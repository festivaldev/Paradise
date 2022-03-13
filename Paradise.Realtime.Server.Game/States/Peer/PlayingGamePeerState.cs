using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Paradise.Realtime.Server.Game {
    public class PlayingGamePeerState : GamePeerState {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlayingGamePeerState));

        public PlayingGamePeerState(GamePeer peer) : base(peer) {

        }

        public override void OnEnter() {
            Peer.Events.Room.SendMatchStart(Room.RoundNumber, Room.EndTime);
        }

        public override void OnResume() { }

        public override void OnExit() { }

        public override void OnUpdate() { }
    }
}
