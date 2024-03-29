﻿using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using System.Collections;
using System.IO;
using UberStrike.Core.ViewModel;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Stores a player's Steam ID in a text file inside the game's data directory for improved handling of multiple instances.
	/// <br>• Prevents new player accounts from connecting to the Comm server before they've set their name.
	/// </summary>
	[HarmonyPatch(typeof(AuthenticationManager))]
	public class AuthenticationManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(AuthenticationManagerHook));

		static AuthenticationManagerHook() {
			Log.Info($"[{nameof(AuthenticationManagerHook)}] hooking {nameof(AuthenticationManager)}");
		}

		[HarmonyPatch("LoginByChannel"), HarmonyPrefix]
		public static bool LoginByChannel_Prefix(AuthenticationManager __instance) {
			string steamId = string.Empty;
			try {
				steamId = File.ReadAllText(string.Join("/", new string[] { Application.dataPath, "PlayerSteamID" }));
			} catch (Exception e) {
				Debug.LogError(e);
			}

			Debug.Log(string.Format("SteamWorks SteamID:{0}, PlayerPrefs SteamID:{1}", PlayerDataManager.SteamId, steamId));

			UnityRuntime.StartRoutine((IEnumerator)ParadiseTraverse.InvokeMethod(__instance, "StartLoginMemberSteam", new object[] { true }));

			return false;
		}

		[HarmonyPatch("CompleteAuthentication"), HarmonyPostfix]
		public static void CompleteAuthentication_Postfix(MemberAuthenticationResultView authView, bool isRegistrationLogin) {
			if (authView.MemberAuthenticationResult == MemberAuthenticationResult.Ok) {
				File.WriteAllText(string.Join("/", new string[] { Application.dataPath, "PlayerSteamID" }), PlayerDataManager.SteamId.ToString());
			}

			if (authView.IsAccountComplete) {
				CommConnectionManagerHook.Connect(AutoMonoBehaviour<CommConnectionManager>.Instance);
			}
		}
	}
}
