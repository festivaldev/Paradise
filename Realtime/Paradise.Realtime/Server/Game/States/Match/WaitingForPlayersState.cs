using Paradise.Core.Types;

namespace Paradise.Realtime.Server.Game {
	internal class WaitingForPlayersState : BaseMatchState {
		public WaitingForPlayersState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;

			Room.SpawnPointManager.Reset();

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				foreach (var player in Room.Players) {
					Room.PreparePlayer(player);
					Room.SpawnPlayer(player, false);

					player.GameEvents.SendWaitingForPlayers();
				}

				if (Room.CanStartMatch) {
					Room.State.SetState(GameStateId.PrepareNextRound);
				}
			} else {
				foreach (var player in Room.Players) {
					player.GameEvents.SendWaitingForPlayers();
				}
			}
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			var player = args.Player;

			Room.PreparePlayer(player);
			Room.SpawnPlayer(player, true);

			player.GameEvents.SendWaitingForPlayers();

			if (Room.CanStartMatch) {
				Room.State.SetState(GameStateId.PrepareNextRound);
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs args) {
			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerKilled(args.AttackerCmid, args.VictimCmid, (byte)args.ItemClass, args.Damage, (byte)args.Part, args.Direction);

				if (peer.Actor.Cmid.CompareTo(args.VictimCmid) == 0) {
					peer.State.SetState(PlayerStateId.Killed);
				}
			}
		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs args) {
			Room.SpawnPlayer(args.Player, false);

			args.Player.State.ResetState();
			args.Player.GameEvents.SendWaitingForPlayers();
		}
	}
}
