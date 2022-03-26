using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Photon.SocketServer;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class GamePeer : BasePeer {
		public GameActor Actor;
		public BaseGameRoom Room;
		public StateMachine<PlayerStateId> State { get; private set; }

		public List<int> KnownActors = new List<int>();

		public UberstrikeUserViewModel Member;
		public LoadoutView Loadout;

		public GamePeerEvents PeerEvents { get; private set; }
		public GameRoomEvents GameEvents => PeerEvents.GameEvents;

		public GamePeer(InitRequest initRequest) : base(initRequest) {
			PeerEvents = new GamePeerEvents(this);

			State = new StateMachine<PlayerStateId>();
			State.RegisterState(PlayerStateId.None, null);
			State.RegisterState(PlayerStateId.Overview, new OverviewPlayerState(this));
			State.RegisterState(PlayerStateId.WaitingForPlayers, new WaitingForPlayersPlayerState(this));
			State.RegisterState(PlayerStateId.Countdown, new CountdownPlayerState(this));
			State.RegisterState(PlayerStateId.Playing, new PlayingPlayerState(this));
			State.RegisterState(PlayerStateId.Killed, new KilledPlayerState(this));
			State.RegisterState(PlayerStateId.AfterRound, new AfterRoundPlayerState(this));
			State.RegisterState(PlayerStateId.Debug, new DebugPlayerState(this));

			AddOperationHandler(new GamePeerOperationsHandler());
		}

		public override void SendHeartbeat(string hash) {
			PeerEvents.SendHeartbeatChallenge(hash);
		}
	}
}
