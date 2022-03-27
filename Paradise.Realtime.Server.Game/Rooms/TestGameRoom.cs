using log4net;
using Paradise.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class TestGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(TestGameRoom));

		public TestGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) { }

		public override bool CanStartMatch => Players.Count > 1;

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();

			// Not pretty, but works for now
			// SendKillsRemaining needs to be called after a player's delta update
			Task.Run(async () => {
				await Task.Delay(30);

				foreach (var peer in Peers) {
					peer.GameEvents.SendKillsRemaining(MetaData.KillLimit - leader.Actor.Info.Kills, leader.Actor.Cmid);
				}

				base.OnPlayerKilled(args);
			});

			if (leader.Actor.Info.Kills >= MetaData.KillLimit) {
				HasRoundEnded = true;
			}
		}
	}
}
