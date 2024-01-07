using HarmonyLib;
using log4net;
using UberStrike.Core.Models;

namespace Paradise.Client {
	/// <summary>
	/// Fixed team assignment in Team Elimination
	/// </summary>
	[HarmonyPatch(typeof(TeamEliminationRoom))]
	public class TeamEliminationRoomHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(TeamEliminationRoomHook));

		static TeamEliminationRoomHook() {
			Log.Info($"[{nameof(TeamEliminationRoomHook)}] hooking {nameof(TeamEliminationRoom)}");
		}

		[HarmonyPatch("OnPlayerJoinedGame"), HarmonyPrefix]
		public static bool OnPlayerJoinedGame_Prefix(TeamEliminationRoom __instance, GameActorInfo player, PlayerMovement position) {
			if (player.Cmid == PlayerDataManager.Cmid) {
				GameState.Current.PlayerData.Team.Value = player.TeamID;
			}

			return true;
		}
	}
}
