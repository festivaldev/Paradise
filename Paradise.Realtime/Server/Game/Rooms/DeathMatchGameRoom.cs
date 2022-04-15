using log4net;
using Paradise.Core.Models;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class DeathMatchGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(DeathMatchGameRoom));

		public DeathMatchGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) { }

		public override bool CanStartMatch => Players.Count > 1;

		public override void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore) {
			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();

			killsRemaining = (short)(MetaData.KillLimit - leader.Actor.Info.Kills);
			blueTeamScore = -1;
			redTeamScore = -1;
		}

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			base.OnPlayerKilled(args);

			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();
			if (leader.Actor.Info.Kills >= MetaData.KillLimit) {
				WinningCmid = leader.Actor.Cmid;

				HasRoundEnded = true;
			}
		}
	}
}
