using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	public static class ParadiseApplicationWebServiceClient {
		public static Coroutine GetCustomMaps(string clientVersion, DefinitionType clientType, Action<List<UberStrikeCustomMapView>> callback, Action<Exception> handler) {
			Coroutine result;

			using (var memoryStream = new MemoryStream()) {
				StringProxy.Serialize(memoryStream, clientVersion);
				EnumProxy<DefinitionType>.Serialize(memoryStream, clientType);

				var mono = Traverse.Create(AccessTools.TypeByName("UberStrike.WebService.Unity.MonoInstance")).Property<MonoBehaviour>("Mono").Value;
				var MakeRequest = AccessTools.TypeByName("UberStrike.WebService.Unity.SoapClient").GetMethod("MakeRequest", BindingFlags.Public | BindingFlags.Static);

				result = mono.StartCoroutine((IEnumerator)MakeRequest.Invoke(null, new object[] {
					"IApplicationWebServiceContract",
					"UberStrike.DataCenter.WebService.CWS.ApplicationWebServiceContract.svc",
					"GetCustomMaps",
					memoryStream.ToArray(),
					(Action<byte[]>)(data => {
						callback?.Invoke(ListProxy<UberStrikeCustomMapView>.Deserialize(new MemoryStream(data), new ListProxy<UberStrikeCustomMapView>.Deserializer<UberStrikeCustomMapView>(UberStrikeCustomMapViewProxy.Deserialize)));
					}),
					handler
				}));
			}

			return result;
		}
	}
}
