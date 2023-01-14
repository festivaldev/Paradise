using HarmonyLib;
using log4net;
using System.Reflection;
using UberStrike.Core.Types;

namespace Paradise.Client {
	public class PlayerLeadAudioHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Disables player lead audio (n kills left, taken/lost/tied for the lead) in Team Elimination games.
		/// </summary>
		public PlayerLeadAudioHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(PlayerLeadAudioHook)}] hooking {nameof(PlayerLeadAudio)}");

			var orig_PlayerLeadAudio_UpdateLeadStatus = typeof(PlayerLeadAudio).GetMethod("UpdateLeadStatus", BindingFlags.Public | BindingFlags.Instance);
			var prefix_PlayerLeadAudio_UpdateLeadStatus = typeof(PlayerLeadAudioHook).GetMethod("UpdateLeadStatus_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_PlayerLeadAudio_UpdateLeadStatus, new HarmonyMethod(prefix_PlayerLeadAudio_UpdateLeadStatus), null);

			var orig_PlayerLeadAudio_PlayKillsLeftAudio = typeof(PlayerLeadAudio).GetMethod("PlayKillsLeftAudio", BindingFlags.Public | BindingFlags.Instance);
			var prefix_PlayerLeadAudio_PlayKillsLeftAudio = typeof(PlayerLeadAudioHook).GetMethod("PlayKillsLeftAudio_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_PlayerLeadAudio_PlayKillsLeftAudio, new HarmonyMethod(prefix_PlayerLeadAudio_PlayKillsLeftAudio), null);
		}

		public static bool UpdateLeadStatus_Prefix() {
			if (GameState.Current.GameMode == GameModeType.EliminationMode ||
				GameState.Current.MatchState.CurrentStateId == GameStateId.PrepareNextRound) return false;

			return true;
		}

		public static bool PlayKillsLeftAudio_Prefix() {
			if (GameState.Current.GameMode == GameModeType.EliminationMode ||
				GameState.Current.MatchState.CurrentStateId == GameStateId.PrepareNextRound) return false;

			return true;
		}
	}
}
