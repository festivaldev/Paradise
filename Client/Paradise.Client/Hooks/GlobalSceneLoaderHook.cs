using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using System.Collections;
using System.Reflection;
using UberStrike.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

namespace Paradise.Client {
	public class GlobalSceneLoaderHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static GlobalSceneLoader Instance;

		private static string ErrorMessage {
			get {
				return GetProperty<string>(Instance, "ErrorMessage", BindingFlags.Static | BindingFlags.Public);
			}

			set {
				SetProperty(Instance, "ErrorMessage", value, BindingFlags.Static | BindingFlags.Public);
			}
		}

		/// <summary>
		/// Reimplements the application authentication flow to reduce the number of potential error messages when launching the game.
		/// </summary>
		public GlobalSceneLoaderHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(GlobalSceneLoaderHook)}] hooking {nameof(GlobalSceneLoader)}");

			var orig_GlobalSceneLoader_Start = typeof(GlobalSceneLoader).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
			var prefix_GlobalSceneLoader_Start = typeof(GlobalSceneLoaderHook).GetMethod("Start_Prefix", BindingFlags.Public | BindingFlags.Static);
			var postfix_GlobalSceneLoader_Start = typeof(GlobalSceneLoaderHook).GetMethod("Start_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_GlobalSceneLoader_Start, new HarmonyMethod(prefix_GlobalSceneLoader_Start), new HarmonyMethod(postfix_GlobalSceneLoader_Start));
		}

		public static bool Start_Prefix() {
			return false;
		}

		public static void Start_Postfix(GlobalSceneLoader __instance) {
			Instance = __instance;

			UnityRuntime.StartRoutine(Start());
		}



		private static IEnumerator Start() {
			Application.runInBackground = true;
			Application.LoadLevel("Menu");

			Configuration.WebserviceBaseUrl = ApplicationDataManager.WebServiceBaseUrl;

			SetProperty(Instance, "GlobalSceneProgress", 1f, BindingFlags.Static | BindingFlags.Public);
			SetProperty(Instance, "IsGlobalSceneLoaded", true, BindingFlags.Static | BindingFlags.Public);
			SetProperty(Instance, "ItemAssetBundleProgress", 1f, BindingFlags.Static | BindingFlags.Public);
			SetProperty(Instance, "IsItemAssetBundleLoaded", true, BindingFlags.Static | BindingFlags.Public);

			InvokeMethod(Instance, "InitializeGlobalScene", null);

			yield return new WaitForSeconds(1f);
			for (float f = 0f; f < 1f; f += Time.deltaTime) {
				yield return new WaitForEndOfFrame();

				var color = GetField<Color>(Instance, "_color");
				color.a = 1f - f / 1f;

				SetField(Instance, "_color", color);
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

						if (!GetField<bool>(Instance, "UseTestPhotonServers")) {
							Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(ev.CommServer);
							Singleton<GameServerManager>.Instance.AddPhotonGameServers(ev.GameServers.FindAll((PhotonView i) => i.UsageType == PhotonUsageType.All));
						} else {
							Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(GetField<string>(Instance, "TestCommServer"), PhotonUsageType.CommServer);
							Singleton<GameServerManager>.Instance.AddTestPhotonGameServer(1000, new PhotonServer(GetField<string>(Instance, "TestGameServer"), PhotonUsageType.All));
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

			yield return new WaitForSeconds(1f);

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

		private static void OpenIosAppStoreUpdatesPage() {
			ApplicationDataManager.OpenUrl(string.Empty, "itms-apps://itunes.com/apps/uberstrike");
		}

		private static void OpenAndroidAppStoreUpdatesPage() {
			ApplicationDataManager.OpenUrl(string.Empty, "market://details?id=com.cmune.uberstrike.android");
		}
	}
}
