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
		private static ParadiseTraverse<GameRoomOperations> traverse;

		static GameRoomOperationsHook() {
			Log.Info($"[{nameof(GameRoomOperationsHook)}] hooking {nameof(GameRoomOperations)}");
		}

		[HarmonyPatch("SendPowerUpRespawnTimes"), HarmonyPrefix]
		public static bool SendPowerUpRespawnTimes_Prefix(GameRoomOperations __instance, List<ushort> respawnTimes) {
			if (traverse == null) {
				traverse = ParadiseTraverse<GameRoomOperations>.Create(__instance);
			}

			var __id = traverse.GetField<byte>("__id");
			var sendOperation = traverse.GetField<RemoteProcedureCall>("sendOperation");
			
			var pickupItems = (Dictionary<int, PickupItem>)AccessTools.Field(typeof(PickupItem), "_instances").GetValue(null);
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
