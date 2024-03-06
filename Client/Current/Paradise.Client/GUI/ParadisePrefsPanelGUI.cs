using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Client {
	internal class ParadisePrefsPanelGUI {
		private static bool showAdvancedSettings;

		private static List<string> WebServiceBaseUrls;
		private static int WebServiceUrlIndex;
		private static string WebServiceEndpoint;
		private static string WebServicePrefix;
		private static string WebServiceSuffix;
		private static List<string> FileServerUrls;
		private static int FileServerUrlIndex;
		private static string ImagePathEndpoint;
		private static string UpdateEndpoint;

		private static bool webServicesDirty;
		private static bool fileServersDirty;
		private static bool updateDirty;

		public static IPopupDialog UpdateDisableConfirmation { get; private set; }

		private static bool HasInvalidWebServerURLs => WebServiceBaseUrls.Find(_ => !Uri.IsWellFormedUriString(_, UriKind.Absolute)) != null;
		private static bool HasInvalidFileServerURLs => FileServerUrls.Find(_ => !Uri.IsWellFormedUriString(_, UriKind.Absolute)) != null;
		public static bool HasInvalidServerURLs => HasInvalidWebServerURLs || HasInvalidFileServerURLs;

		public static void Draw() {
			GUITools.PushGUIState();

			GUI.contentColor = ColorScheme.UberStrikeYellow;
			GUILayout.Label("Changing some of the following settings may require UberStrike to be restarted.", BlueStonez.label_interparkbold_11pt_left);
			GUI.contentColor = Color.white;

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			#region General
			ParadiseGUITools.DrawGroup("General", delegate {
				// Discord Rich Presence
				var enableDiscordRPC = GUILayout.Toggle(ParadiseClient.Settings.EnableDiscordRichPresence, "Enable Discord Rich Presence", BlueStonez.toggle);

				if (enableDiscordRPC != ParadiseClient.Settings.EnableDiscordRichPresence) {
					ParadiseClient.Settings.EnableDiscordRichPresence = enableDiscordRPC;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Show detailed item statistics in the shop
				var showDetailedItemStatistics = GUILayout.Toggle(ParadiseClient.Settings.ShowDetailedItemStatistics, "Show detailed item statistics", BlueStonez.toggle);

				if (showDetailedItemStatistics != ParadiseClient.Settings.ShowDetailedItemStatistics) {
					ParadiseClient.Settings.ShowDetailedItemStatistics = showDetailedItemStatistics;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Show weapon names im killfeed
				var showKilledByWeapon = GUILayout.Toggle(ParadiseClient.Settings.ShowKilledWeaponIndicator, "Show weapon names in killfeed", BlueStonez.toggle);

				if (showKilledByWeapon != ParadiseClient.Settings.ShowKilledWeaponIndicator) {
					ParadiseClient.Settings.ShowKilledWeaponIndicator = showKilledByWeapon;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Enable landing grunt
				var enableLandingGrunt = GUILayout.Toggle(ParadiseClient.Settings.EnableLandingGrunt, "Enable landing grunt sound effect", BlueStonez.toggle);

				if (enableLandingGrunt != ParadiseClient.Settings.EnableLandingGrunt) {
					ParadiseClient.Settings.EnableLandingGrunt = enableLandingGrunt;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Advanced Settings
				showAdvancedSettings = GUILayout.Toggle(showAdvancedSettings, "Show advanced settings", BlueStonez.toggle);

				if (showAdvancedSettings) {
					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					GUI.contentColor = Color.red;
					GUILayout.Label("These settings are for experienced users only.\nIf you set incorrect values, you can break things.\n\nProceed at your own risk.", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;
				}
			});
			#endregion

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			#region Web Service Settings
			ParadiseGUITools.DrawGroup("Servers", delegate {
				// Web Services
				GUILayout.BeginHorizontal();

				GUILayout.Label("Web Services", BlueStonez.label_interparkbold_11pt_left);

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Add", BlueStonez.buttondark_small, GUILayout.Width(36f), GUILayout.Height(20f))) {
					WebServiceBaseUrls.Add(string.Empty);
					webServicesDirty = true;
				};

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				GUI.enabled = webServicesDirty && !HasInvalidWebServerURLs;
				if (GUILayout.Button("Save", BlueStonez.buttondark_small, GUILayout.Width(40f), GUILayout.Height(20f))) {
					SaveWebServiceSettings();
				}
				GUI.enabled = true;

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				foreach (var item in WebServiceBaseUrls.ToList().Select((x, i) => new { Value = x, Index = i })) {
					var isSelectedServer = item.Index == WebServiceUrlIndex;
					var isValidUrl = Uri.IsWellFormedUriString(item.Value, UriKind.Absolute);

					if (item.Index > 0) GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					GUILayout.BeginHorizontal();

					GUI.enabled = isValidUrl;
					var selectedServer = GUILayout.Toggle(isSelectedServer, string.Empty, BlueStonez.radiobutton, GUILayout.Width(16f), GUILayout.Height(22f));
					if (selectedServer && selectedServer != isSelectedServer) {
						WebServiceUrlIndex = item.Index;
						webServicesDirty = true;
					}
					GUI.enabled = true;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

					if (!isValidUrl) {
						GUI.contentColor = Color.red;
					}

					var value = GUILayout.TextField(item.Value, BlueStonez.textField, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
					GUI.contentColor = Color.white;

					if (value != item.Value) {
						WebServiceBaseUrls[item.Index] = value;
						webServicesDirty = true;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

					GUI.enabled = item.Index != WebServiceUrlIndex;
					if (GUILayout.Button("x", BlueStonez.buttondark_small, GUILayout.Width(22f), GUILayout.Height(22f))) {
						WebServiceBaseUrls.RemoveAt(item.Index);
						webServicesDirty = true;

						if (WebServiceUrlIndex > item.Index) {
							WebServiceUrlIndex--;
						}
					}
					GUI.enabled = true;

					GUILayout.EndHorizontal();
				}

				if (showAdvancedSettings) {
					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var serviceEndpoint = WebServiceEndpoint;
					ParadiseGUITools.DrawTextField("WebService Endpoint", ref serviceEndpoint);

					if (serviceEndpoint != WebServiceEndpoint) {
						WebServiceEndpoint = serviceEndpoint;
						webServicesDirty = true;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var servicePrefix = WebServicePrefix;
					ParadiseGUITools.DrawTextField("WebService Prefix", ref servicePrefix);

					if (servicePrefix != WebServicePrefix) {
						WebServicePrefix = servicePrefix;
						webServicesDirty = true;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var serviceSuffix = WebServiceSuffix;
					ParadiseGUITools.DrawTextField("WebService Suffix", ref serviceSuffix);

					if (serviceSuffix != WebServiceSuffix) {
						WebServiceSuffix = serviceSuffix;
						webServicesDirty = true;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var encryptWSTraffic = GUILayout.Toggle(ParadiseClient.Settings.EncryptWebServiceTraffic, "Enable WebService traffic encryption", BlueStonez.toggle);

					if (encryptWSTraffic != ParadiseClient.Settings.EncryptWebServiceTraffic) {
						ParadiseClient.Settings.EncryptWebServiceTraffic = encryptWSTraffic;
					}
				}

				GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

				// File Servers
				GUILayout.BeginHorizontal();

				GUILayout.Label("File Servers", BlueStonez.label_interparkbold_11pt_left);

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Add", BlueStonez.buttondark_small, GUILayout.Width(36f), GUILayout.Height(20f))) {
					FileServerUrls.Add(string.Empty);
					fileServersDirty = true;
				};

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				GUI.enabled = fileServersDirty && !HasInvalidFileServerURLs;
				if (GUILayout.Button("Save", BlueStonez.buttondark_small, GUILayout.Width(40f), GUILayout.Height(20f))) {
					SaveFileServerSettings();
				}
				GUI.enabled = true;

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				foreach (var item in FileServerUrls.ToList().Select((x, i) => new { Value = x, Index = i })) {
					var isSelectedServer = item.Index == FileServerUrlIndex;
					var isValidUrl = Uri.IsWellFormedUriString(item.Value, UriKind.Absolute);

					if (item.Index > 0) GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					GUILayout.BeginHorizontal();

					GUI.enabled = isValidUrl;
					var selectedServer = GUILayout.Toggle(isSelectedServer, string.Empty, BlueStonez.radiobutton, GUILayout.Width(16f), GUILayout.Height(22f));
					if (selectedServer && selectedServer != isSelectedServer) {
						FileServerUrlIndex = item.Index;
						fileServersDirty = true;
					}
					GUI.enabled = true;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

					if (!isValidUrl) {
						GUI.contentColor = Color.red;
					}

					var value = GUILayout.TextField(item.Value, BlueStonez.textField, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
					GUI.contentColor = Color.white;

					if (value != item.Value) {
						FileServerUrls[item.Index] = value;
						fileServersDirty = true;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

					GUI.enabled = item.Index != FileServerUrlIndex;
					if (GUILayout.Button("x", BlueStonez.buttondark_small, GUILayout.Width(22f), GUILayout.Height(22f))) {
						FileServerUrls.RemoveAt(item.Index);
						fileServersDirty = true;

						if (FileServerUrlIndex > item.Index) {
							FileServerUrlIndex--;
						}
					}
					GUI.enabled = true;

					GUILayout.EndHorizontal();
				}

				if (showAdvancedSettings) {
					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var imagePathEndpoint = ImagePathEndpoint;

					ParadiseGUITools.DrawTextField("Image Path Endpoint", ref imagePathEndpoint);

					if (imagePathEndpoint != ImagePathEndpoint) {
						ImagePathEndpoint = imagePathEndpoint;
						fileServersDirty = true;
					}
				}
			});
			#endregion

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			#region Update Settings
			ParadiseGUITools.DrawGroup("Servers", delegate {
				// Update Toggle
				var enableUpdates = GUILayout.Toggle(ParadiseClient.Settings.AutoUpdates, "Enable automatic updates", BlueStonez.toggle);

				if (enableUpdates != ParadiseClient.Settings.AutoUpdates) {
					if (!enableUpdates) {
						UpdateDisableConfirmation = PopupSystem.ShowMessage("Disable Automatic Updates", "Are you sure you want to disable automatic updates?\n\nYou will no longer receive bug fixes and new features unless you turn on updates again.", PopupSystem.AlertType.OKCancel, delegate {
							ParadiseClient.Settings.AutoUpdates = enableUpdates;
							UpdateDisableConfirmation = null;
						}, "Yes", delegate {
							UpdateDisableConfirmation = null;
						}, "No");
					} else {
						ParadiseClient.Settings.AutoUpdates = enableUpdates;
					}
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUITools.PushGUIState();
				GUI.enabled = ParadiseClient.Settings.AutoUpdates;

				// Update Channel
				var updateChannel = (int)ParadiseClient.Settings.UpdateChannel;
				ParadiseGUITools.DrawToolbar("Update Channel", ref updateChannel, new string[] { "Stable", "Beta" });

				if (updateChannel != (int)ParadiseClient.Settings.UpdateChannel) {
					ParadiseClient.Settings.UpdateChannel = (UpdateChannel)updateChannel;
				}

				GUI.enabled = true;

				if (showAdvancedSettings) {

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					GUI.enabled = fileServersDirty && !HasInvalidFileServerURLs;
					if (GUILayout.Button("Save", BlueStonez.buttondark_small, GUILayout.Width(40f), GUILayout.Height(20f))) {
						SaveFileServerSettings();
					}
					GUI.enabled = true;

					GUILayout.EndHorizontal();

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var updateEndpoint = UpdateEndpoint;
					ParadiseGUITools.DrawTextField("Update Endpoint", ref updateEndpoint);

					if (updateEndpoint != UpdateEndpoint) {
						UpdateEndpoint = updateEndpoint;
						updateDirty = true;
					}
				}

				GUITools.PopGUIState();
			});
			#endregion

			GUITools.PopGUIState();
		}

		public static void ReloadSettings() {
			WebServiceBaseUrls = ParadiseClient.Settings.GetKey<List<string>>(ParadisePrefs.Key.WebServiceBaseUrls);
			WebServiceUrlIndex = ParadiseClient.Settings.GetKey<int>(ParadisePrefs.Key.WebServiceUrlIndex);
			WebServiceEndpoint = ParadiseClient.Settings.GetKey<string>(ParadisePrefs.Key.WebServiceEndpoint);
			WebServicePrefix = ParadiseClient.Settings.GetKey<string>(ParadisePrefs.Key.WebServicePrefix);
			WebServiceSuffix = ParadiseClient.Settings.GetKey<string>(ParadisePrefs.Key.WebServiceSuffix);

			FileServerUrls = ParadiseClient.Settings.GetKey<List<string>>(ParadisePrefs.Key.FileServerUrls);
			FileServerUrlIndex = ParadiseClient.Settings.GetKey<int>(ParadisePrefs.Key.FileServerUrlIndex);
			ImagePathEndpoint = ParadiseClient.Settings.GetKey<string>(ParadisePrefs.Key.ImagePathEndpoint);

			UpdateEndpoint = ParadiseClient.Settings.GetKey<string>(ParadisePrefs.Key.UpdateEndpoint);
		}

		public static void SaveSettings() {
			SaveWebServiceSettings();
			SaveFileServerSettings();
			SaveUpdateSettings();
		}

		private static void SaveWebServiceSettings() {
			if (webServicesDirty) {
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.WebServiceBaseUrls, WebServiceBaseUrls);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.WebServiceUrlIndex, WebServiceUrlIndex);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.WebServiceEndpoint, WebServiceEndpoint);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.WebServiceSuffix, WebServiceSuffix);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.WebServicePrefix, WebServicePrefix);
			}

			webServicesDirty = false;
		}

		private static void SaveFileServerSettings() {
			if (fileServersDirty) {
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.FileServerUrls, FileServerUrls);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.FileServerUrlIndex, FileServerUrlIndex);
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.ImagePathEndpoint, ImagePathEndpoint);
			}

			fileServersDirty = false;
		}

		private static void SaveUpdateSettings() {
			if (updateDirty) {
				ParadiseClient.Settings.SetKey(ParadisePrefs.Key.UpdateEndpoint, UpdateEndpoint);
			}

			updateDirty = false;
		}
	}
}
