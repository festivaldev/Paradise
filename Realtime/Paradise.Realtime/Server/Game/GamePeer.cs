using Photon.SocketServer;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.Realtime.Server.Game {
	public partial class GamePeer : BasePeer {
		public struct ShotData {
			public int StartTime;
			public int EndTime;
			public int WeaponID;
		}


		public GameActor Actor;
		public BaseGameRoom Room;
		public StateMachine<PlayerStateId> State { get; private set; }

		public LoadoutView Loadout;

		public GamePeer.EventSender PeerEventSender { get; private set; }
		public BaseGameRoom.EventSender GameEventSender => PeerEventSender.GameEventSender;

		public ShotData Shooting = new ShotData();

		public GamePeer(InitRequest initRequest) : base(initRequest) {
			PeerEventSender = new EventSender(this);

			State = new StateMachine<PlayerStateId>();
			State.RegisterState(PlayerStateId.None, null);
			State.RegisterState(PlayerStateId.Overview, new PlayerOverviewState(this));
			State.RegisterState(PlayerStateId.PrepareForMatch, new PlayerPrepareState(this));
			State.RegisterState(PlayerStateId.Playing, new PlayerPlayingState(this));
			State.RegisterState(PlayerStateId.Killed, new PlayerKilledState(this));
			State.RegisterState(PlayerStateId.Spectating, new PlayerSpectatingState(this));

			State.SetState(PlayerStateId.None);

			AddOperationHandler(new OperationHandler());
		}

		public override void SendHeartbeat(string hash) {
			PeerEventSender.SendHeartbeatChallenge(hash);
		}

		public override void SendError(string message = "An error occured that forced UberStrike to halt.") {
			base.SendError(message);
			PeerEventSender.SendDisconnectAndDisablePhoton(message);
		}

		public bool HasSpawnedOnSpawnPoint(SpawnPoint spawnPoint) {
			if (Actor.PreviousSpawnPoints.Count == 0) return false;

			foreach (var point in Actor.PreviousSpawnPoints) {
				if (point.Position.Equals(spawnPoint.Position) && point.Rotation.Equals(spawnPoint.Rotation)) {
					return true;
				}
			}

			return false;
		}

		public override string ToString() {
			return $"GamePeer[{Actor?.ActorInfo?.PlayerName}({Actor?.ActorInfo?.Cmid})]";
		}
	}
}
