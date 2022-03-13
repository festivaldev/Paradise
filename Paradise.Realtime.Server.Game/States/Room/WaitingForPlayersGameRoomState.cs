using log4net;

namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(WaitingForPlayersGameRoomState));

		public WaitingForPlayersGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }

		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			var player = e.Player;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.Events.Room.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
				otherPeer.KnownActors.Add(e.Player.Actor.Cmid);
			}

			if (Room.Players.Count > 1) {
				Room.State.SetState(GameRoomState.Id.Countdown);
			} else {
				player.State.SetState(GamePeerState.Id.WaitingForPlayers);
			}
		}
	}
}
