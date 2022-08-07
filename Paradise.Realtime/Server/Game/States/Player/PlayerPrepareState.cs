using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerPrepareState : BasePlayerState {
		public PlayerPrepareState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.GameEvents.SendPrepareNextRound();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
