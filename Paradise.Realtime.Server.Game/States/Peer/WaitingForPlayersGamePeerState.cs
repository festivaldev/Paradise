using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Paradise.Realtime.Server.Game {
    public class WaitingForPlayersGamePeerState : GamePeerState {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaitingForPlayersGamePeerState));

        public WaitingForPlayersGamePeerState(GamePeer peer) : base(peer) {

        }

        public override void OnEnter() {
            Peer.Events.Room.SendWaitingForPlayers();
            Peer.Events.Room.SendMatchStartCountdown(4);
        }

        public override void OnResume() { }

        public override void OnExit() { }

        public override void OnUpdate() { }
    }
}
