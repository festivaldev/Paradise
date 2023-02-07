using HarmonyLib;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using UberStrike.Core.Models;
using UberStrike.Realtime.Client;
using UnityEngine;

namespace Paradise.Client {
	[ServiceContract]
	public interface RpcServiceHost {
		[OperationContract]
		void ClearPresence();

		[OperationContract]
		void SetPresence(string details, string state);

		[OperationContract]
		void SetPresenceWithEndTimestamp(string details, string state, DateTime end);

		[OperationContract]
		void SetPresenceWithStartEnd(string details, string state, DateTime start, DateTime end);
	}

	public class RichPresenceClient : MonoBehaviour {
		private static readonly ILog Log = LogManager.GetLogger(nameof(RichPresenceClient));

		private static Process childProcess;
		private static RpcServiceHost serviceProxy;

		public static void Initialize() {
			if (!ParadiseClient.EnableDiscordRichPresence) return;

			var harmonyInstance = new Harmony("tf.festival.Paradise.DiscordRPC");

			#region Discord-specific patches
			var orig_MenuPageManager_LoadPage = typeof(MenuPageManager).GetMethod("LoadPage", BindingFlags.Public | BindingFlags.Instance);
			var postfix_MenuPageManager_LoadPage = typeof(RichPresenceClient).GetMethod("MenuPageManager_LoadPage_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_MenuPageManager_LoadPage, null, new HarmonyMethod(postfix_MenuPageManager_LoadPage));


			var postfix_IState_OnEnter = typeof(RichPresenceClient).GetMethod("IState_OnEnter_Postfix", BindingFlags.Public | BindingFlags.Static);

			foreach (var type in (new string[] {
				"PregameLoadoutState",
				"WaitingForPlayersState",
				"PrepareNextRoundState",
				"MatchRunningState",
				"EndOfMatchState",
				"AfterRoundState"
			})) {
				var orig_IState_OnEnter = typeof(ApplicationDataManager).Assembly.GetType(type).GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance);

				harmonyInstance.Patch(orig_IState_OnEnter, null, new HarmonyMethod(postfix_IState_OnEnter));
			}

			var postfix_BaseGameRoom_OnKillsRemaining = typeof(RichPresenceClient).GetMethod("BaseGameRoom_OnKillsRemaining_Postfix", BindingFlags.Public | BindingFlags.Static);
			var postfix_BaseGameRoom_OnUpdateRoundScore = typeof(RichPresenceClient).GetMethod("BaseGameRoom_OnUpdateRoundScore_Postfix", BindingFlags.Public | BindingFlags.Static);

			foreach (var type in (new string[] {
				"DeathMatchRoom",
				"TeamDeathMatchRoom",
				"TeamEliminationRoom"
			})) {
				var orig_BaseGameRoom_OnKillsRemaining = typeof(ApplicationDataManager).Assembly.GetType(type).GetMethod("OnKillsRemaining", BindingFlags.NonPublic | BindingFlags.Instance);
				var orig_BaseGameRoom_OnUpdateRoundScore = typeof(ApplicationDataManager).Assembly.GetType(type).GetMethod("OnUpdateRoundScore", BindingFlags.NonPublic | BindingFlags.Instance);

				harmonyInstance.Patch(orig_BaseGameRoom_OnKillsRemaining, null, new HarmonyMethod(postfix_BaseGameRoom_OnKillsRemaining));
				harmonyInstance.Patch(orig_BaseGameRoom_OnUpdateRoundScore, null, new HarmonyMethod(postfix_BaseGameRoom_OnUpdateRoundScore));
			}
			#endregion

			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.FileName = Path.Combine(Application.dataPath, @"Plugins\Paradise.Client.DiscordRPC.exe");
			processStartInfo.UseShellExecute = false;
			processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
			processStartInfo.CreateNoWindow = true;
			childProcess = Process.Start(processStartInfo);

			var binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.None;

			ChannelFactory<RpcServiceHost> pipeFactory = new ChannelFactory<RpcServiceHost>(binding, new EndpointAddress("net.tcp://localhost/NewParadise.Client.DiscordRPC"));

			serviceProxy = pipeFactory.CreateChannel();
		}

		public static void Dispose() {
			ClearPresence();

			childProcess.Kill();
		}

		#region Service Methods
		public static void ClearPresence() {
			serviceProxy?.ClearPresence();
		}

		public static void SetPresence(string details, string state) {
			serviceProxy?.SetPresence(details, state);
		}

		public static void SetPresence(string details, string state, DateTime end) {
			serviceProxy?.SetPresenceWithEndTimestamp(details, state, end);
		}

		public static void SetPresence(string details, string state, DateTime start, DateTime end) {
			serviceProxy?.SetPresenceWithStartEnd(details, state, start, end);
		}
		#endregion

		public static void MenuPageManager_LoadPage_Postfix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			string presenceState = string.Empty;

			switch (pageType) {
				case PageType.Home:
					presenceState = "Home Screen";
					break;

				case PageType.Play:
					presenceState = "Browsing Servers";
					break;

				case PageType.Stats:
					presenceState = "Looking at Statistics";
					break;

				case PageType.Shop:
					presenceState = "Buying Gear";
					break;

				case PageType.Inbox:
					presenceState = "Reading Inbox Messages";
					break;

				case PageType.Clans:
					presenceState = "Messaging Clan Members";
					break;

				case PageType.Training:
					presenceState = "Exploring Maps";
					break;

				case PageType.Login:
					presenceState = "DEBUG: Login";
					break;

				case PageType.Chat:
					presenceState = "Chatting with Players";
					break;

				default: break;
			}

			SetPresence("Main Menu", presenceState);
		}

		public static void IState_OnEnter_Postfix(IState __instance) {
			//var mapName = Singleton<MapManager>.Instance.GetMapName(Singleton<SceneLoader>.Instance.CurrentScene);

			//// Training Room
			//if (Singleton<GameStateController>.Instance.CurrentGameMode == GameMode.Training || Singleton<GameStateController>.Instance.CurrentGameMode == GameMode.None) {
			//	SetPresence($"Exploring {mapName}", "");
			//} else {
			//	var gameMode = string.Empty;

			//	switch (Singleton<GameStateController>.Instance.CurrentGameMode) {
			//		case GameMode.DeathMatch:
			//			gameMode = "Deathmatch";
			//			break;
			//		case GameMode.TeamDeathMatch:
			//			gameMode = "Team Deathmatch";
			//			break;
			//		case GameMode.TeamElimination:
			//			gameMode = "Team Elimination";
			//			break;
			//		default: break;
			//	}

			//	switch (__instance.GetType().Name) {
			//		case "PregameLoadoutState":
			//			SetPresence($"{gameMode} - {mapName}", "Selecting loadout");
			//			break;
			//		case "WaitingForPlayersState":
			//			SetPresence($"{gameMode} - {mapName}", "Waiting for players");
			//			break;
			//		case "PrepareNextRoundState":
			//			SetPresence($"{gameMode} - {mapName}", "Waiting for round to start");
			//			break;
			//		case "MatchRunningState":
			//			if (GameState.Current.IsTeamGame) {
			//				var teamName = string.Empty;

			//				switch (GameState.Current.PlayerData.Team.Value) {
			//					case TeamID.BLUE:
			//						teamName = "Blue";
			//						break;
			//					case TeamID.RED:
			//						teamName = "Red";
			//						break;
			//					default: break;
			//				}

			//				SetPresence($"{gameMode} - {mapName}", $"Team {teamName} - [ {GameState.Current.ScoreBlue} : {GameState.Current.ScoreRed} ]", DateTime.UtcNow.AddSeconds(GameState.Current.RoomData.TimeLimit));
			//			} else {
			//				SetPresence($"{gameMode} - {mapName}", $"Playing - {GameState.Current.PlayerData.RemainingKills} kill(s) remaining", DateTime.UtcNow.AddSeconds(GameState.Current.RoomData.TimeLimit));
			//			}

			//			break;
			//		case "EndOfMatchState":
			//			SetPresence($"{gameMode} - {mapName}", "Selecting loadout");
			//			break;
			//		case "AfterRoundState":
			//			SetPresence($"{gameMode} - {mapName}", "Selecting loadout");
			//			break;
			//		default: break;
			//	}
			//}

			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			var titleString = TitleString(gameMode, currentScene);

			if (gameMode == GameMode.Training || gameMode == GameMode.None) {
				SetPresence(titleString, "");
			} else {
				switch (__instance.GetType().Name) {
					case "PregameLoadoutState":
						SetPresence(titleString, "Selecting loadout");
						break;
					case "WaitingForPlayersState":
						SetPresence(titleString, "Waiting for players");
						break;
					case "PrepareNextRoundState":
						SetPresence(titleString, "Waiting for round to start");
						break;
					case "MatchRunningState":
						SetPresence(titleString, ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, GameState.Current.PlayerData.RemainingKills, GameState.Current.ScoreBlue, GameState.Current.ScoreRed), GetRoundEndTime());

						break;
					case "AfterRoundState":
						SetPresence(titleString, "Round ended");
						break;
					case "EndOfMatchState":
						SetPresence(titleString, "Viewing round statistics");
						break;
					default: break;
				}
			}
		}

		public static void BaseGameRoom_OnKillsRemaining_Postfix(BaseGameRoom __instance, int killsRemaining, int leaderCmid) {
			if (GameState.Current.MatchState.CurrentStateId != GameStateId.MatchRunning) return;

			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			var titleString = TitleString(gameMode, currentScene);
			SetPresence(titleString, ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, killsRemaining, -1, -1), GetRoundEndTime());
		}

		public static void BaseGameRoom_OnUpdateRoundScore_Postfix(BaseGameRoom __instance, int round, short blue, short red) {
			if (GameState.Current.MatchState.CurrentStateId != GameStateId.MatchRunning) return;

			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			var titleString = TitleString(gameMode, currentScene);
			SetPresence(titleString, ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, -1, blue, red), GetRoundEndTime());
		}

		private static string TitleString(GameMode gameMode, string scene) {
			var mapName = Singleton<MapManager>.Instance.GetMapName(scene);
			var gameModeName = string.Empty;

			switch (Singleton<GameStateController>.Instance.CurrentGameMode) {
				case GameMode.TeamDeathMatch:
					gameModeName = "Team Deathmatch";
					break;
				case GameMode.DeathMatch:
					gameModeName = "Deathmatch";
					break;
				case GameMode.TeamElimination:
					gameModeName = "Team Elimination";
					break;
				default: break;
			}

			switch (gameMode) {
				case GameMode.None:
				case GameMode.Training:
					return $"Exploring {mapName}";
				default: return $"{gameModeName} - {mapName}";
			}
		}

		private static string ScoreString(GameMode gameMode, TeamID team, int killsRemaining, int scoreBlue, int scoreRed) {
			var teamName = string.Empty;

			switch (GameState.Current.PlayerData.Team.Value) {
				case TeamID.BLUE:
					teamName = "Blue";
					break;
				case TeamID.RED:
					teamName = "Red";
					break;
				default: break;
			}

			if (gameMode != GameMode.DeathMatch) {
				return $"Team {teamName} - [ {scoreBlue} : {scoreRed} ]";
			} else {
				return $"Playing - {killsRemaining} kill(s) remaining";
			}
		}

		private static DateTime GetRoundEndTime() {
			var roundCurrentTime = Singleton<GameStateController>.Instance.Client.ServerTimeTicks;
			var roundStartTime = (int)typeof(GameState).GetField("roundStartTime", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GameState.Current);
			var roundEndTime = roundStartTime + GameState.Current.RoomData.TimeLimit * 1000;

			return DateTime.UtcNow.AddMilliseconds((roundEndTime - roundStartTime) - (roundCurrentTime - roundStartTime));
		}
	}
}
