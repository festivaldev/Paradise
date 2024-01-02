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

		private static ParadiseTraverse<GlobalSceneLoader> traverse;

		private static string ErrorMessage {
			get {
				return traverse.GetProperty<string>("ErrorMessage");
			}

			set {
				traverse.SetProperty("ErrorMessage", value);
			}
		}

		static GlobalSceneLoaderHook() {
			Log.Info($"[{nameof(GlobalSceneLoaderHook)}] hooking {nameof(GlobalSceneLoader)}");
		}

		[HarmonyPatch("Start"), HarmonyPrefix]
		public static bool GlobalSceneLoader_Start_Prefix(GlobalSceneLoader __instance) {
			traverse = ParadiseTraverse<GlobalSceneLoader>.Create(__instance);

			return false;
		}

		[HarmonyPatch("Start"), HarmonyPostfix]
		public static void GlobalSceneLoader_Start_Postfix(GlobalSceneLoader __instance) {
			traverse.Instance.StartCoroutine(StartWithCheckingUpdates());
		}

		#region
		private static IEnumerator StartWithCheckingUpdates() {
			AutoMonoBehaviour<PreloadOptionsPanelButton>.Instance.enabled = true;

			UnityRuntime.StartRoutine(ParadiseUpdater.CleanupUpdates());

			UnityRuntime.StartRoutine(AutoMonoBehaviour<ParadiseUpdater>.Instance.CheckForUpdatesIfNecessary((updateCatalog) => {
				if (updateCatalog != null) {
					ParadiseUpdater.HandleUpdateAvailable(updateCatalog, () => {
						traverse.Instance.StartCoroutine(Start());
					});
				} else {
					traverse.Instance.StartCoroutine(Start());
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

			traverse.SetProperty("GlobalSceneProgress", 1f);
			traverse.SetProperty("IsGlobalSceneLoaded", true);
			traverse.SetProperty("ItemAssetBundleProgress", 1f);
			traverse.SetProperty("IsItemAssetBundleLoaded", true);

			traverse.InvokeMethod("InitializeGlobalScene");

			for (float f = 0f; f < 1f; f += Time.deltaTime) {
				yield return new WaitForEndOfFrame();

				var color = traverse.GetField<Color>("_color");
				color.a = 1f - f / 1f;

				traverse.SetField("_color", color);
			}

			bool continueAuthentication = true;

			yield return traverse.Instance.StartCoroutine(BeginAuthenticateApplication(delegate (AuthenticateApplicationView ev) {
				try {
					GlobalSceneLoader.IsInitialised = true;
					if (ev != null && ev.IsEnabled) {
						Configuration.EncryptionInitVector = ev.EncryptionInitVector;
						Configuration.EncryptionPassPhrase = ev.EncryptionPassPhrase;
						ApplicationDataManager.IsOnline = true;

						Debug.Log("OnAuthenticateApplication");

						Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(ev.CommServer);
						Singleton<GameServerManager>.Instance.AddPhotonGameServers(ev.GameServers.FindAll((PhotonView i) => i.UsageType == PhotonUsageType.All));


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
			}, delegate (Exception e) {
				continueAuthentication = false;

				OnAuthenticateApplicationException(e);
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
		#endregion
	}
}
