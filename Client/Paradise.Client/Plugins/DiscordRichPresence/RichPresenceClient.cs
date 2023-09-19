using HarmonyLib;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UberStrike.Core.Models;
using UberStrike.Realtime.Client;
using UnityEngine;

namespace Paradise.Client {
	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_BASIC_LIMIT_INFORMATION {
		public Int64 PerProcessUserTimeLimit;
		public Int64 PerJobUserTimeLimit;
		public UInt32 LimitFlags;
		public UIntPtr MinimumWorkingSetSize;
		public UIntPtr MaximumWorkingSetSize;
		public UInt32 ActiveProcessLimit;
		public UIntPtr Affinity;
		public UInt32 PriorityClass;
		public UInt32 SchedulingClass;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
		public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
		public IO_COUNTERS IoInfo;
		public UIntPtr ProcessMemoryLimit;
		public UIntPtr JobMemoryLimit;
		public UIntPtr PeakProcessMemoryUsed;
		public UIntPtr PeakJobMemoryUsed;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct IO_COUNTERS {
		public UInt64 ReadOperationCount;
		public UInt64 WriteOperationCount;
		public UInt64 OtherOperationCount;
		public UInt64 ReadTransferCount;
		public UInt64 WriteTransferCount;
		public UInt64 OtherTransferCount;
	}

	public enum JobObjectInfoType {
		AssociateCompletionPortInformation = 7,
		BasicLimitInformation = 2,
		BasicUIRestrictions = 4,
		EndOfJobTimeInformation = 6,
		ExtendedLimitInformation = 9,
		SecurityLimitInformation = 5,
		GroupInformation = 11
	}

	[HarmonyPatch]
	internal class RichPresenceClient {
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInformationClass, IntPtr lpJobObjectInformation, uint cbJobObjectInformationLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool CloseHandle(IntPtr handle);


		private static readonly ILog Log = LogManager.GetLogger(nameof(RichPresenceClient));
		private static readonly string PresenceFile = Path.Combine(Path.GetTempPath(), "7b37f0ce-4a3e-4735-a713-2bf27277ad74");

		private static Process childProcess;

		public static void Initialize() {
			if (!ParadiseClient.Settings.EnableDiscordRichPresence) return;
			if (!File.Exists(Path.Combine(Application.dataPath, @"Plugins\Paradise.Client.DiscordRPC.exe"))) return;

			#region Discord-specific patches
			var harmonyInstance = new Harmony("tf.festival.Paradise.DiscordRPC");

			var postfix_IState_OnEnter = typeof(RichPresenceClient).GetMethod("IState_OnEnter_Postfix", BindingFlags.Public | BindingFlags.Static);

			foreach (var type in new string[] {
				"PregameLoadoutState",
				"WaitingForPlayersState",
				"PrepareNextRoundState",
				"MatchRunningState",
				"EndOfMatchState",
				"AfterRoundState"
			}) {
				var orig_IState_OnEnter = AccessTools.Method(typeof(ApplicationDataManager).Assembly.GetType(type), "OnEnter");

				harmonyInstance.Patch(orig_IState_OnEnter, null, new HarmonyMethod(postfix_IState_OnEnter));
			}

			var postfix_BaseGameRoom_OnKillsRemaining = typeof(RichPresenceClient).GetMethod("BaseGameRoom_OnKillsRemaining_Postfix", BindingFlags.Public | BindingFlags.Static);
			var postfix_BaseGameRoom_OnUpdateRoundScore = typeof(RichPresenceClient).GetMethod("BaseGameRoom_OnUpdateRoundScore_Postfix", BindingFlags.Public | BindingFlags.Static);

			foreach (var type in new string[] {
				"DeathMatchRoom",
				"TeamDeathMatchRoom",
				"TeamEliminationRoom"
			}) {
				var orig_BaseGameRoom_OnKillsRemaining = typeof(ApplicationDataManager).Assembly.GetType(type).GetMethod("OnKillsRemaining", BindingFlags.NonPublic | BindingFlags.Instance);
				var orig_BaseGameRoom_OnUpdateRoundScore = typeof(ApplicationDataManager).Assembly.GetType(type).GetMethod("OnUpdateRoundScore", BindingFlags.NonPublic | BindingFlags.Instance);

				harmonyInstance.Patch(orig_BaseGameRoom_OnKillsRemaining, null, new HarmonyMethod(postfix_BaseGameRoom_OnKillsRemaining));
				harmonyInstance.Patch(orig_BaseGameRoom_OnUpdateRoundScore, null, new HarmonyMethod(postfix_BaseGameRoom_OnUpdateRoundScore));
			}
			#endregion

			try {
				var handle = CreateJobObject(IntPtr.Zero, null);
				var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION {
					LimitFlags = 0x2000
				};

				var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
					BasicLimitInformation = info
				};

				int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
				IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
				Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

				if (!SetInformationJobObject(handle, (int)JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length)) {
					throw new Exception(string.Format("Unable to set job object information.  Error: {0}", Marshal.GetLastWin32Error()));
				}

				ProcessStartInfo processStartInfo = new ProcessStartInfo {
					FileName = Path.Combine(Application.dataPath, @"Plugins\Paradise.Client.DiscordRPC.exe"),
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Minimized,
					CreateNoWindow = true
				};

				childProcess = Process.Start(processStartInfo);

				AssignProcessToJobObject(handle, childProcess.Handle);

				SetPresence(new RichPresenceSerializable {
					Details = "Playing UberStrike"
				});
			} catch (Exception e) {
				Log.Error(e);
			}
		}

		public static void Dispose() {
			ClearPresence();

			childProcess.Kill();

			if (File.Exists(PresenceFile)) {
				File.Delete(PresenceFile);
			}
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

		public static void ClearPresence() {
			File.WriteAllText(PresenceFile, SerializePresence(new RichPresenceSerializable {
				ClearPresence = true
			}));
		}


		public static void SetPresence(RichPresenceSerializable presence) {
			if (presence.Assets == null) {
				presence.Assets = new AssetsSerializable {
					LargeImageKey = "uberstrike"
				};
			}

			var _presence = SerializePresence(presence);

			using (var stream = File.Open(PresenceFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
				var bytes = Encoding.UTF8.GetBytes(_presence);
				stream.Write(bytes, 0, bytes.Length);
				stream.Close();
			}
		}

		private static string SerializePresence(RichPresenceSerializable presence) {
			var serializer = new XmlSerializer(typeof(RichPresenceSerializable));
			using (var stringWriter = new StringWriter()) {
				using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter)) {
					serializer.Serialize(xmlWriter, presence);
					return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringWriter.ToString()));
				}
			}
		}

		#region Additional hooks
		[HarmonyPatch(typeof(MenuPageManager), "LoadPage"), HarmonyPostfix]
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

			SetPresence(new RichPresenceSerializable {
				Details = "Main Menu",
				State = presenceState
			});
		}

		public static void IState_OnEnter_Postfix(IState __instance) {
			var gameRoom = GameState.Current.RoomData;
			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			if (gameMode == GameMode.Training || gameMode == GameMode.None) {
				var mapName = Singleton<MapManager>.Instance.GetMapName(currentScene);

				SetPresence(new RichPresenceSerializable {
					Details = "Training",
					State = mapName,
					Timestamps = new TimestampsSerializable {
						Start = DateTime.UtcNow
					}
				});
			} else {
				var titleString = TitleString(gameMode, currentScene);

				if (__instance.GetType().Name == "MatchRunningState") {
					SetPresence(new RichPresenceSerializable {
						Details = titleString,
						State = ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, GameState.Current.PlayerData.RemainingKills, GameState.Current.ScoreBlue, GameState.Current.ScoreRed),
						Timestamps = new TimestampsSerializable {
							End = GetRoundEndTime()
						},
						Buttons = new ButtonSerializable[] {
							new ButtonSerializable {
								Label = "Join Game",
								Url = $"uberstrike://connect/{gameRoom.Server.ConnectionString}/{gameRoom.Number}"
							}
						}
					});
				} else {
					string presenceState = string.Empty;

					switch (__instance.GetType().Name) {
						case "PregameLoadoutState":
							presenceState = "Selecting loadout";
							break;
						case "WaitingForPlayersState":
							presenceState = "Waiting for players";
							break;
						case "PrepareNextRoundState":
							presenceState = "Waiting for round to start";
							break;
						case "AfterRoundState":
							presenceState = "Round ended";
							break;
						case "EndOfMatchState":
							presenceState = "Viewing round statistics";
							break;
						default: break;
					}

					SetPresence(new RichPresenceSerializable {
						Details = titleString,
						State = presenceState
					});
				}
			}
		}

		public static void BaseGameRoom_OnKillsRemaining_Postfix(BaseGameRoom __instance, int killsRemaining, int leaderCmid) {
			if (GameState.Current.MatchState.CurrentStateId != GameStateId.MatchRunning) return;

			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			var titleString = TitleString(gameMode, currentScene);

			SetPresence(new RichPresenceSerializable {
				Details = titleString,
				State = ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, killsRemaining, -1, -1),
				Timestamps = new TimestampsSerializable {
					End = GetRoundEndTime()
				}
			});
		}

		public static void BaseGameRoom_OnUpdateRoundScore_Postfix(BaseGameRoom __instance, int round, short blue, short red) {
			if (GameState.Current.MatchState.CurrentStateId != GameStateId.MatchRunning) return;

			var gameMode = Singleton<GameStateController>.Instance.CurrentGameMode;
			var currentScene = Singleton<SceneLoader>.Instance.CurrentScene;

			var titleString = TitleString(gameMode, currentScene);

			SetPresence(new RichPresenceSerializable {
				Details = titleString,
				State = ScoreString(gameMode, GameState.Current.PlayerData.Team.Value, -1, blue, red),
				Timestamps = new TimestampsSerializable {
					End = GetRoundEndTime()
				}
			});
		}
		#endregion
	}
}
