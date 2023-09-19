using ExitGames.Client.Photon;
using System.Linq;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugServerStatePanel : IDebugPage {
		public string Title => "Network";

		public void Draw() {
			ParadiseGUITools.DrawGroup("Comm Server", delegate {
				var commPeer = AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Peer;

				if (commPeer.PeerState != PeerStateValue.Connected) {
					GUI.contentColor = ColorScheme.UberStrikeYellow;
					GUILayout.Label("You're not connected to a Comm server.", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);
				}

				GUI.enabled = commPeer.PeerState == PeerStateValue.Connected;
				ParadiseGUITools.DrawTextField("Address", commPeer.ServerAddress);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Peer State", commPeer.PeerState);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Server Time", commPeer.ServerTimeInMilliSeconds);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Inbound Traffic", ParadiseGUITools.FormatSize(commPeer.BytesIn));
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Outbound Traffic", ParadiseGUITools.FormatSize(commPeer.BytesOut));
				GUI.enabled = true;
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Game Server", delegate {
				var gamePeer = Singleton<GameStateController>.Instance.Client.Peer;

				if (gamePeer.PeerState != PeerStateValue.Connected) {
					GUI.contentColor = ColorScheme.UberStrikeYellow;
					GUILayout.Label("You're not connected to a Game server.", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);
				}

				GUI.enabled = gamePeer.PeerState == PeerStateValue.Connected;
				ParadiseGUITools.DrawTextField("Address", gamePeer.ServerAddress);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Peer State", gamePeer.PeerState);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Server Time", gamePeer.ServerTimeInMilliSeconds);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Inbound Traffic", ParadiseGUITools.FormatSize(gamePeer.BytesIn));
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Outbound Traffic", ParadiseGUITools.FormatSize(gamePeer.BytesOut));
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(Singleton<GameStateController>.Instance.Client.IsInsideRoom, "Is In Room", GUILayout.Height(22f));
				GUI.enabled = true;
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("All Servers", delegate {
				if (Singleton<GameServerManager>.Instance.PhotonServerList.Count() == 0) {
					GUI.enabled = false;
					GUILayout.Label("Please open the \"Play\" menu first.", BlueStonez.label_interparkbold_11pt_left);
					GUI.enabled = true;

					return;
				}

				foreach (var item in Singleton<GameServerManager>.Instance.PhotonServerList.Select((x, i) => new { Value = x, Index = i })) {
					if (item.Index > 0) {
						GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);
					}

					var photonServer = item.Value;

					GUILayout.Label($"({photonServer.Id}) {photonServer.Name}", BlueStonez.label_interparkbold_11pt_left);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Address", photonServer.ConnectionString);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Min Latency", photonServer.MinLatency);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Latency", photonServer.Latency);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Region", photonServer.Region);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				}
			});
		}
	}
}
