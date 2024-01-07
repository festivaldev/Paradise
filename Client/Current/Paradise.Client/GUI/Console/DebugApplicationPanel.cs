using System.Linq;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugApplicationPanel : IDebugPage {
		public string Title => "Application";

		public void Draw() {
			ParadiseGUITools.DrawGroup("Application Info", delegate {
				ParadiseGUITools.DrawTextField("Channel", ApplicationDataManager.Channel);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Version", ApplicationDataManager.Version);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Source", Application.srcValue);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("WS API", UberStrike.DataCenter.UnitySdk.ApiVersion.Current);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("RT API", UberStrike.Realtime.UnitySdk.ApiVersion.Current);
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("User Info", delegate {
				GUILayout.Label("Player Info", BlueStonez.label_interparkbold_11pt_left);
				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				ParadiseGUITools.DrawTextField("Player Name", PlayerDataManager.Name);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Email", PlayerDataManager.Email);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Steam ID", PlayerDataManager.SteamId);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Cmid", PlayerDataManager.Cmid);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Access Level", PlayerDataManager.AccessLevel);

				GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

				GUILayout.Label("Player Statistics/Wallet", BlueStonez.label_interparkbold_11pt_left);
				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				ParadiseGUITools.DrawTextField("XP", PlayerDataManager.PlayerExperience);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Level", PlayerDataManager.PlayerLevel);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Credits", PlayerDataManager.Credits);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Points", PlayerDataManager.Points);

				GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

				GUILayout.Label("Clan Info", BlueStonez.label_interparkbold_11pt_left);

				var isPlayerInClan = PlayerDataManager.IsPlayerInClan;

				if (!isPlayerInClan) {
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					GUI.contentColor = ColorScheme.UberStrikeYellow;
					GUILayout.Label("You're not in a clan!", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUITools.PushGUIState();
				GUI.enabled = isPlayerInClan;

				ParadiseGUITools.DrawTextField("Clan ID", PlayerDataManager.ClanID);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Clan Name", PlayerDataManager.ClanName);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Clan Tag", PlayerDataManager.ClanTag);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Clan Motto", PlayerDataManager.ClanMotto);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Clan Owner", PlayerDataManager.ClanOwnerName);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Clan Rank", Singleton<PlayerDataManager>.Instance.RankInClan);

				GUI.enabled = true;
				GUITools.PopGUIState();
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Server Info", delegate {
				var commServer = Singleton<GameServerManager>.Instance.CommServer;
				var gameServers = Singleton<GameServerManager>.Instance.PhotonServerList;

				GUILayout.Label("Comm Server", BlueStonez.label_interparkbold_11pt_left);
				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				ParadiseGUITools.DrawTextField("Name", commServer.Name);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("ID", commServer.Id);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Connection String", commServer.ConnectionString);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Region", commServer.Region);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Latency", commServer.Latency);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Min Latency", commServer.MinLatency);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				ParadiseGUITools.DrawTextField("Server Load", commServer.ServerLoad);

				GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

				if (gameServers.Count() == 0) {
					GUILayout.Label("Game Server", BlueStonez.label_interparkbold_11pt_left);
					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					GUI.contentColor = ColorScheme.UberStrikeYellow;
					GUILayout.Label("No game servers to show", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;
				} else {
					foreach (var item in gameServers.ToList().Select((x, i) => new { Value = x, Index = i })) {
						GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

						GUILayout.Label($"Game Server #{item.Index + 1}", BlueStonez.label_interparkbold_11pt_left);
						GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

						ParadiseGUITools.DrawTextField("Name", item.Value.Name);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("ID", item.Value.Id);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("Connection String", item.Value.ConnectionString);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("Region", item.Value.Region);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("Latency", item.Value.Latency);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("Min Latency", item.Value.MinLatency);

						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						ParadiseGUITools.DrawTextField("Server Load", item.Value.ServerLoad);
					}
				}
			});
		}
	}
}
