using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	public class ParadiseApplicationWebServiceClient {
		public static string WebServiceName => "ApplicationWebService";
		public static string ContractName => "IApplicationWebServiceContract";

		public static Coroutine GetCustomMaps(string clientVersion, DefinitionType clientType, Action<List<ParadiseMapView>> callback, Action<Exception> handler) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				StringProxy.Serialize(memoryStream, clientVersion);
				EnumProxy<DefinitionType>.Serialize(memoryStream, clientType);

				var mono = Traverse.Create(AccessTools.TypeByName("UberStrike.WebService.Unity.MonoInstance")).Property<MonoBehaviour>("Mono").Value;
				var MakeRequest = AccessTools.TypeByName("UberStrike.WebService.Unity.SoapClient").GetMethod("MakeRequest", BindingFlags.Public | BindingFlags.Static);


				return mono.StartCoroutine((IEnumerator)MakeRequest.Invoke(null, new object[] {
					ContractName,
					$"{ParadiseClient.Settings.WebServicePrefix}{WebServiceName}{ParadiseClient.Settings.WebServiceSuffix}",
					MethodBase.GetCurrentMethod().Name,
					memoryStream.ToArray(),
					(Action<byte[]>)(data => {
						using (var stream = new MemoryStream(data)) {
							callback?.Invoke(ListProxy<ParadiseMapView>.Deserialize(stream, ParadiseMapViewProxy.Deserialize));
						}
					}),
					handler
				}));
			}
		}
	}
}
