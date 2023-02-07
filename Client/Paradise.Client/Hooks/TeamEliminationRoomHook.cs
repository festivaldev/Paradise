using HarmonyLib;
using log4net;
using System.Reflection;
using UberStrike.Core.Models;

namespace Paradise.Client {
	public class TeamEliminationRoomHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Fixed team assignment in Team Elimination
		/// </summary>
		public TeamEliminationRoomHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(TeamEliminationRoomHook)}] hooking {nameof(TeamEliminationRoom)}");

			var orig_TeamEliminationRoom_OnPlayerJoinedGame = typeof(TeamEliminationRoom).GetMethod("OnPlayerJoinedGame", BindingFlags.NonPublic | BindingFlags.Instance);
			var prefix_TeamEliminationRoom_OnPlayerJoinedGame = typeof(TeamEliminationRoomHook).GetMethod("OnPlayerJoinedGame_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_TeamEliminationRoom_OnPlayerJoinedGame, new HarmonyMethod(prefix_TeamEliminationRoom_OnPlayerJoinedGame), null);
		}

		public static bool OnPlayerJoinedGame_Prefix(TeamEliminationRoom __instance, GameActorInfo player, PlayerMovement position) {
			if (player.Cmid == PlayerDataManager.Cmid) {
				GameState.Current.PlayerData.Team.Value = player.TeamID;
			}

			return true;
		}
	}
}
