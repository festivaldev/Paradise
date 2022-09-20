using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Photon.SocketServer;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class GamePeer : BasePeer {
		public Guid PeerId { get; } = Guid.NewGuid();

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

			State.SetState(PlayerStateId.None);

			AddOperationHandler(new GamePeerOperationHandler());
		}

		public override void SendHeartbeat(string hash) {
			PeerEvents.SendHeartbeatChallenge(hash);
		}

		public bool HasSpawnedOnSpawnPoint(SpawnPoint spawnPoint) {
			if (PreviousSpawnPoints.Count == 0) return false;

			foreach (var point in PreviousSpawnPoints) {
				if (point.Position.Equals(spawnPoint.Position) && point.Rotation.Equals(spawnPoint.Rotation)) {
					return true;
				}
			}

			return false;
		}
	}
}
