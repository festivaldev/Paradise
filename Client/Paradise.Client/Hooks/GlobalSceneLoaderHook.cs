using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using System.Collections;
using UberStrike.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Reimplements the application authentication flow to reduce the number of potential error messages when launching the game.
	/// </summary>
	[HarmonyPatch(typeof(GlobalSceneLoader))]
	public class GlobalSceneLoaderHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(GlobalSceneLoaderHook));

		private static GlobalSceneLoader Instance;
		private static Traverse traverse;

		private static string ErrorMessage {
			get {
				return GetProperty<string>("ErrorMessage");
			}

			set {
				SetProperty("ErrorMessage", value);
			}
		}

		static GlobalSceneLoaderHook() {
			Log.Info($"[{nameof(GlobalSceneLoaderHook)}] hooking {nameof(GlobalSceneLoader)}");
		}

		[HarmonyPatch("Start"), HarmonyPrefix]
		public static bool GlobalSceneLoader_Start_Prefix(GlobalSceneLoader __instance) {
			if (Instance == null) {
				Instance = __instance;
				traverse = Traverse.Create(__instance);
			}

			return false;
		}

		[HarmonyPatch("Start"), HarmonyPostfix]
		public static void GlobalSceneLoader_Start_Postfix(GlobalSceneLoader __instance) {
			UnityRuntime.StartRoutine(StartWithCheckingUpdates());
		}

		private static IEnumerator StartWithCheckingUpdates() {
			UnityRuntime.StartRoutine(ParadiseUpdater.CleanupUpdates());

			UnityRuntime.StartRoutine(GameObject.Find("Plugin Holder").GetComponent<ParadiseUpdater>().CheckForUpdatesIfNecessary((updateCatalog) => {
				if (updateCatalog != null) {
					ParadiseUpdater.HandleUpdateAvailable(updateCatalog, () => {
						UnityRuntime.StartRoutine(Start());
					});
				} else {
					UnityRuntime.StartRoutine(Start());
				}
			}, (error) => {
				Log.Error(error);
				ParadiseUpdater.HandleUpdateError(error, () => {
					Application.Quit();
				});
			}));

			yield break;
		}

		private static IEnumerator Start() {
			Application.runInBackground = true;
			Application.LoadLevel("Menu");

			Configuration.WebserviceBaseUrl = ApplicationDataManager.WebServiceBaseUrl;

			SetProperty("GlobalSceneProgress", 1f);
			SetProperty("IsGlobalSceneLoaded", true);
			SetProperty("ItemAssetBundleProgress", 1f);
			SetProperty("IsItemAssetBundleLoaded", true);

			InvokeMethod("InitializeGlobalScene");

			//yield return new WaitForSeconds(1f);
			for (float f = 0f; f < 1f; f += Time.deltaTime) {
				yield return new WaitForEndOfFrame();

				var color = GetField<Color>("_color");
				color.a = 1f - f / 1f;

				SetField("_color", color);
			}

			bool continueAuthentication = true;

			yield return UnityRuntime.StartRoutine(BeginAuthenticateApplication(delegate (AuthenticateApplicationView ev) {
				try {
					GlobalSceneLoader.IsInitialised = true;
					if (ev != null && ev.IsEnabled) {
						Configuration.EncryptionInitVector = ev.EncryptionInitVector;
						Configuration.EncryptionPassPhrase = ev.EncryptionPassPhrase;
						ApplicationDataManager.IsOnline = true;

						Debug.Log("OnAuthenticateApplication");

						if (!GetField<bool>("UseTestPhotonServers")) {
							Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(ev.CommServer);
							Singleton<GameServerManager>.Instance.AddPhotonGameServers(ev.GameServers.FindAll((PhotonView i) => i.UsageType == PhotonUsageType.All));
						} else {
							Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(GetField<string>("TestCommServer"), PhotonUsageType.CommServer);
							Singleton<GameServerManager>.Instance.AddTestPhotonGameServer(1000, new PhotonServer(GetField<string>("TestGameServer"), PhotonUsageType.All));
						}

						if (ev.WarnPlayer) {
							continueAuthentication = false;

							HandleVersionWarning();
						}
					} else {
						continueAuthentication = false;

						Debug.Log($"OnAuthenticateApplication failed with 4.7.1/{ApplicationDataManager.Channel}: {GlobalSceneLoader.ErrorMessage}");

						GlobalSceneLoaderHook.ErrorMessage = "Please update.";

						HandleVersionError();
					}
				} catch (Exception ex) {
					continueAuthentication = false;

					GlobalSceneLoaderHook.ErrorMessage = ex.Message + " " + ex.StackTrace;

					Debug.LogError($"OnAuthenticateApplication crashed with 4.7.1/{ApplicationDataManager.Channel}: {GlobalSceneLoader.ErrorMessage}");
					HandleApplicationAuthenticationError("There was a problem loading UberStrike. Please check your internet connection and try again.");
				}
			}, delegate (Exception exception) {
				continueAuthentication = false;

				OnAuthenticateApplicationException(exception);
			}));

			if (!continueAuthentication) yield break;

			Debug.Log("Start LoginByChannel");

			if (PlayerDataManager.IsTestBuild) {
				PopupSystem.ShowMessage("Warning", "This is a test build, do not distribute!", PopupSystem.AlertType.OK, delegate () {
					Singleton<AuthenticationManager>.Instance.LoginByChannel();
				});
			} else {
				Singleton<AuthenticationManager>.Instance.LoginByChannel();
			}

			// yield return new WaitForSeconds(1f);

			yield break;
		}

		private static IEnumerator BeginAuthenticateApplication(Action<AuthenticateApplicationView> callback, Action<Exception> errorCallback) {
			Debug.Log("BeginAuthenticateApplication " + Configuration.WebserviceBaseUrl);

			yield return ApplicationWebServiceClient.AuthenticateApplication("4.7.1", ApplicationDataManager.Channel, string.Empty, delegate (AuthenticateApplicationView authView) {
				Debug.Log("Connected to : " + Configuration.WebserviceBaseUrl);

				callback(authView);
			}, delegate (Exception exception) {
				errorCallback(exception);
			});

			yield break;
		}

		private static void OnAuthenticateApplicationException(Exception exception) {
			ErrorMessage = exception.Message;

			Debug.LogError($"An exception occurred while authenticating the application with 4.7.1/{ApplicationDataManager.Channel}: {exception.Message}");
			HandleApplicationAuthenticationError("There was a problem loading UberStrike. Please check your internet connection and try again.");
		}

		private static void RetryAuthentiateApplication() {
			//GlobalSceneLoaderHook.ErrorMessage = string.Empty;
			//UnityRuntime.StartRoutine(BeginAuthenticateApplication());
		}

		private static void HandleApplicationAuthenticationError(string message) {
			ChannelType channel = ApplicationDataManager.Channel;
			switch (channel) {
				case ChannelType.Steam:
				case ChannelType.WindowsStandalone:
				case ChannelType.OSXStandalone:
					PopupSystem.ShowError(LocalizedStrings.Error, message + " Failed to connect to the UberStrike Web Services.", PopupSystem.AlertType.OK, new Action(Application.Quit));
					break;
				case ChannelType.IPhone:
				case ChannelType.IPad:
				case ChannelType.Android:
					PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, new Action(RetryAuthentiateApplication));
					break;
				default:
					if (channel != ChannelType.WebPortal && channel != ChannelType.WebFacebook) {
						PopupSystem.ShowError(LocalizedStrings.Error, message + " This client type is not supported.", PopupSystem.AlertType.OK, new Action(Application.Quit));
					} else {
						PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.None);
					}
					break;
			}
		}

		private static void HandleVersionWarning() {
			ChannelType channel = ApplicationDataManager.Channel;
			switch (channel) {
				case ChannelType.Steam:
					PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Please update from the Steam store.", PopupSystem.AlertType.OKCancel, new Action(OpenSteamStorePage), new Action(Singleton<AuthenticationManager>.Instance.LoginByChannel));
					break;
				case ChannelType.IPhone:
				case ChannelType.IPad:
					PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from the App Store.", PopupSystem.AlertType.OKCancel, new Action(OpenIosAppStoreUpdatesPage), new Action(Singleton<AuthenticationManager>.Instance.LoginByChannel));
					break;
				case ChannelType.Android:
					PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from our website.", PopupSystem.AlertType.OKCancel, new Action(OpenAndroidAppStoreUpdatesPage), new Action(Singleton<AuthenticationManager>.Instance.LoginByChannel));
					break;
				default:
					if (channel != ChannelType.WebPortal && channel != ChannelType.WebFacebook) {
						PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is not supported. Please update from our website.\n(Invalid Channel: " + ApplicationDataManager.Channel + ")", PopupSystem.AlertType.OK);
					} else {
						PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. You should refresh your browser.", PopupSystem.AlertType.OK, new Action(Singleton<AuthenticationManager>.Instance.LoginByChannel));
					}
					break;
			}
		}

		private static void HandleVersionError() {
			ChannelType channel = ApplicationDataManager.Channel;
			switch (channel) {
				case ChannelType.Steam:
					PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Please update from the Steam store.", PopupSystem.AlertType.OK, new Action(OpenSteamStorePage));
					break;
				case ChannelType.IPhone:
				case ChannelType.IPad:
					PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from the App Store.", PopupSystem.AlertType.OK, new Action(OpenIosAppStoreUpdatesPage));
					break;
				case ChannelType.Android:
					PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from our website.", PopupSystem.AlertType.OK, new Action(OpenAndroidAppStoreUpdatesPage));
					break;
				default:
					if (channel != ChannelType.WebPortal && channel != ChannelType.WebFacebook) {
						PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is not supported. Please update from our website.\n(Invalid Channel: " + ApplicationDataManager.Channel + ")", PopupSystem.AlertType.OK);
					} else {
						PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please refresh your browser.", PopupSystem.AlertType.None);
					}
					break;
			}
		}

		private static void OpenSteamStorePage() {
			ApplicationDataManager.OpenUrl(string.Empty, "steam://store/291210");
			Application.Quit();
		}

		private static void OpenIosAppStoreUpdatesPage() {
			ApplicationDataManager.OpenUrl(string.Empty, "itms-apps://itunes.com/apps/uberstrike");
			Application.Quit();
		}

		private static void OpenAndroidAppStoreUpdatesPage() {
			ApplicationDataManager.OpenUrl(string.Empty, "market://details?id=com.cmune.uberstrike.android");
			Application.Quit();
		}

		#region 
		private static T GetField<T>(string fieldName) {
			return traverse.Field<T>(fieldName).Value;
		}

		private static void SetField(string fieldName, object value) {
			traverse.Field(fieldName).SetValue(value);
		}

		private static T GetProperty<T>(string propertyName) {
			return traverse.Property<T>(propertyName).Value;
		}

		private static void SetProperty(string propertyName, object value) {
			traverse.Property(propertyName).SetValue(value);
		}

		private static object InvokeMethod(string methodName, params object[] parameters) {
			return AccessTools.Method(Instance.GetType(), methodName).Invoke(Instance, parameters);
		}
		#endregion
	}
}
