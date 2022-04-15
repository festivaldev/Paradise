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
		public List<SpawnPoint> PreviousSpawnPoints = new List<SpawnPoint>();

		public UberstrikeUserViewModel Member;
		public LoadoutView Loadout;

		public GamePeerEvents PeerEvents { get; private set; }
		public GameRoomEvents GameEvents => PeerEvents.GameEvents;

		public int ShootingStartTime;
		public int ShootingEndTime;
		public int ShootingWeapon;

		public GamePeer(InitRequest initRequest) : base(initRequest) {
			PeerEvents = new GamePeerEvents(this);

			State = new StateMachine<PlayerStateId>();
			State.RegisterState(PlayerStateId.None, null);
			State.RegisterState(PlayerStateId.Overview, new PlayerOverviewState(this));
			State.RegisterState(PlayerStateId.PrepareForMatch, new PlayerPrepareState(this));
			State.RegisterState(PlayerStateId.Playing, new PlayerPlayingState(this));
			State.RegisterState(PlayerStateId.Killed, new PlayerKilledState(this));

			AddOperationHandler(new GamePeerOperationsHandler());
		}

		public override void SendHeartbeat(string hash) {
			PeerEvents.SendHeartbeatChallenge(hash);
		}
	}
}
