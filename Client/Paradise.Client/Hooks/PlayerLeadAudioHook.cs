using HarmonyLib;
using log4net;
using UberStrike.Core.Types;

namespace Paradise.Client {
	/// <summary>
	/// Disables player lead audio (n kills left, taken/lost/tied for the lead) in Team Elimination games.
	/// </summary>
	[HarmonyPatch(typeof(PlayerLeadAudio))]
	public class PlayerLeadAudioHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(PlayerLeadAudioHook));

		static PlayerLeadAudioHook() {
			Log.Info($"[{nameof(PlayerLeadAudioHook)}] hooking {nameof(PlayerLeadAudio)}");
		}

		[HarmonyPatch("UpdateLeadStatus"), HarmonyPrefix]
		public static bool UpdateLeadStatus_Prefix() {
			if (GameState.Current.GameMode == GameModeType.EliminationMode ||
				GameState.Current.MatchState.CurrentStateId == GameStateId.PrepareNextRound) return false;

			return true;
		}

		[HarmonyPatch("PlayKillsLeftAudio"), HarmonyPrefix]
		public static bool PlayKillsLeftAudio_Prefix() {
			if (GameState.Current.GameMode == GameModeType.EliminationMode ||
				GameState.Current.MatchState.CurrentStateId == GameStateId.PrepareNextRound) return false;

			return true;
		}
	}
}
