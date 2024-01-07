using UberStrike.Core.Models;
using UberStrike.Core.Types;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerSpectatingState : BasePlayerState {
		public PlayerSpectatingState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.ActorInfo.PlayerState |= PlayerStates.Spectator;

			if (Room.State.CurrentStateId == GameStateId.MatchRunning) {
				Peer.GameEventSender.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);

				if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
					short killsRemaining = (short)Room.MetaData.KillLimit;

					Room.GetCurrentScore(out killsRemaining, out _, out _);
					Peer.GameEventSender.SendKillsRemaining(killsRemaining, 0);
				} else {
					Room.GetCurrentScore(out _, out short blueTeamScore, out short redTeamScore);
					Peer.GameEventSender.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}

			Peer.GameEventSender.SendJoinedAsSpectator();
		}

		public override void OnExit() {
			Peer.Actor.ActorInfo.PlayerState &= ~PlayerStates.Spectator;
		}

		public override void OnResume() { }

		public override void OnUpdate() {

		}
	}
}
