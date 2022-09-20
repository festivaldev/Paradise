using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UberStrike.Core.ViewModel;
using UnityEngine;

namespace Paradise.Client {
	public class AuthenticationManagerHook : IParadiseHook {
		/// <summary>
		/// Stores a player's Steam ID in a text file inside the game's data directory for improved handling of multiple instances.
		/// </summary>
		public AuthenticationManagerHook() { }

		public void Hook(Harmony harmonyInstance) {
			var orig_AuthenticationManager_LoginByChannel = typeof(AuthenticationManager).GetMethod("LoginByChannel", BindingFlags.Public | BindingFlags.Instance);
			var prefix_AuthenticationManager_LoginByChannel = typeof(AuthenticationManagerHook).GetMethod("LoginByChannel_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_AuthenticationManager_LoginByChannel, new HarmonyMethod(prefix_AuthenticationManager_LoginByChannel), null);

			var orig_AuthenticationManager_CompleteAuthentication = typeof(AuthenticationManager).GetMethod("CompleteAuthentication", BindingFlags.NonPublic | BindingFlags.Instance);
			var postfix_AuthenticationManager_CompleteAuthentication = typeof(AuthenticationManagerHook).GetMethod("CompleteAuthentication_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_AuthenticationManager_CompleteAuthentication, null, new HarmonyMethod(postfix_AuthenticationManager_CompleteAuthentication));
		}

		public static bool LoginByChannel_Prefix(AuthenticationManager __instance) {
			var loginMethod = typeof(AuthenticationManager).GetMethod("StartLoginMemberSteam", BindingFlags.Public | BindingFlags.Instance);

			string steamId = string.Empty;
			try {
				steamId = File.ReadAllText(string.Join("/", new string[] { Application.dataPath, "PlayerSteamID" }));
			} catch (Exception e) {
				Debug.LogError(e);
			}

			Debug.Log(string.Format("SteamWorks SteamID:{0}, PlayerPrefs SteamID:{1}", PlayerDataManager.SteamId, steamId));

			if (string.IsNullOrEmpty(steamId) || steamId != PlayerDataManager.SteamId) {
				Debug.Log(string.Format("No SteamID saved. Using SteamWorks SteamID:{0}", PlayerDataManager.SteamId));

				PopupSystem.ShowMessage(string.Empty, "Have you played UberStrike before?", PopupSystem.AlertType.OKCancel, delegate () {
					UnityRuntime.StartRoutine((IEnumerator)loginMethod.Invoke(__instance, new object[] { true }));
				}, "No", delegate () {
					PopupSystem.ShowMessage(string.Empty, "Do you want to upgrade an UberStrike.com or Facebook account?\n\nNOTE: This will permenantly link your UberStrike account to this Steam ID", PopupSystem.AlertType.OKCancel, delegate () {
						UnityRuntime.StartRoutine((IEnumerator)loginMethod.Invoke(__instance, new object[] { true }));
					}, "No", delegate () {
						UnityRuntime.StartRoutine((IEnumerator)loginMethod.Invoke(__instance, new object[] { false }));
					}, "Yes");
				}, "Yes");
			} else {
				Debug.Log(string.Format("Login using saved SteamID:{0}", steamId));
				UnityRuntime.StartRoutine((IEnumerator)loginMethod.Invoke(__instance, new object[] { true }));
			}

			return false;
		}

		public static void CompleteAuthentication_Postfix(MemberAuthenticationResultView authView, bool isRegistrationLogin) {
			if (authView.MemberAuthenticationResult == MemberAuthenticationResult.Ok) {
				File.WriteAllText(string.Join("/", new string[] { Application.dataPath, "PlayerSteamID" }), PlayerDataManager.SteamId.ToString());
			}
		}
	}
}
