using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UberStrike.Core.Serialization;
using UnityEngine;

namespace Paradise.Client {
	public class ParadiseUserWebServiceClient {
		public static string WebServiceName => "UserWebService";
		public static string ContractName => "IUserWebServiceContract";

		public static Coroutine RemoveItemFromInventory(int itemId, string authToken, Action<int> callback, Action<Exception> handler) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, itemId);
				StringProxy.Serialize(memoryStream, authToken);

				var mono = Traverse.Create(AccessTools.TypeByName("UberStrike.WebService.Unity.MonoInstance")).Property<MonoBehaviour>("Mono").Value;
				var MakeRequest = AccessTools.TypeByName("UberStrike.WebService.Unity.SoapClient").GetMethod("MakeRequest", BindingFlags.Public | BindingFlags.Static);

				return mono.StartCoroutine((IEnumerator)MakeRequest.Invoke(null, new object[] {
					ContractName,
					$"{ParadiseClient.Settings.WebServicePrefix}{WebServiceName}{ParadiseClient.Settings.WebServiceSuffix}",
					MethodBase.GetCurrentMethod().Name,
					memoryStream.ToArray(),
					(Action<byte[]>)(data => {
						callback?.Invoke(Int32Proxy.Deserialize(new MemoryStream(data)));
					}),
					handler
				}));
			}
		}
	}
}
