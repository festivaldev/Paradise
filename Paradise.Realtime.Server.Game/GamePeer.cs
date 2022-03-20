using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Photon.SocketServer;

namespace Paradise.Realtime.Server.Game {
	public class GamePeer : BasePeer {
		public UberstrikeUserViewModel Member;
		public LoadoutView Loadout;
		public GameActor Actor;
		public BaseGameRoom Room;

		public GamePeerEvents Events { get; private set; }

		public StateMachine<GamePeerState.Id> State { get; private set; }

		public GamePeer(InitRequest initRequest) : base(initRequest) {
			Events = new GamePeerEvents(this);

			State = new StateMachine<GamePeerState.Id>();
			State.RegisterState(GamePeerState.Id.None, null);
			State.RegisterState(GamePeerState.Id.Debug, new DebugGamePeerState(this));
			State.RegisterState(GamePeerState.Id.Overview, new OverviewGamePeerState(this));
			State.RegisterState(GamePeerState.Id.WaitingForPlayers, new WaitingForPlayersGamePeerState(this));
			State.RegisterState(GamePeerState.Id.Countdown, new CountdownGamePeerState(this));
			State.RegisterState(GamePeerState.Id.Playing, new PlayingGamePeerState(this));
			State.RegisterState(GamePeerState.Id.Killed, new KilledGamePeerState(this));

			AddOperationHandler(new GamePeerOperationsHandler());
		}
	}
}
