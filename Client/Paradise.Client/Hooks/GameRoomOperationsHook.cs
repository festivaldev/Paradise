using HarmonyLib;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberStrike.Core.Serialization;
using UberStrike.Realtime.Client;
using UnityEngine;

namespace Paradise.Client.Hooks {
	/// <summary>
	/// <br>Makes the game send pickup positions alongside respawn times</br>
	/// </summary>
	[HarmonyPatch(typeof(GameRoomOperations))]
	public class GameRoomOperationsHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ApplicationDataManagerHook));

		static GameRoomOperationsHook() {
			Log.Info($"[{nameof(GameRoomOperationsHook)}] hooking {nameof(GameRoomOperations)}");
		}

		[HarmonyPatch("SendPowerUpRespawnTimes"), HarmonyPrefix]
		public static bool GameRoomOperations_SendPowerUpRespawnTimes_Prefix(GameRoomOperations __instance, List<ushort> respawnTimes) {
			var __id = Traverse.Create(__instance).Field("__id").GetValue<byte>();
			var sendOperation = Traverse.Create(__instance).Field("sendOperation").GetValue<RemoteProcedureCall>();

			var pickupItems = Traverse.Create(typeof(PickupItem)).Field("_instances").GetValue<Dictionary<int, PickupItem>>();
			var positions = pickupItems.Select(_ => _.Value.transform.position).ToList();


			using (MemoryStream memoryStream = new MemoryStream()) {
				ListProxy<ushort>.Serialize(memoryStream, respawnTimes, new ListProxy<ushort>.Serializer<ushort>(UInt16Proxy.Serialize));
				ListProxy<Vector3>.Serialize(memoryStream, positions, Vector3Proxy.Serialize);

				Dictionary<byte, object> customOpParameters = new Dictionary<byte, object> {
					{
						__id,
						memoryStream.ToArray()
					}
				};

				sendOperation?.Invoke(3, customOpParameters, true, 0, false);
			}

			return false;
		}
	}
}
