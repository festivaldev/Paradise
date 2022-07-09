using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	public static class ParadiseApplicationWebServiceClient {
		public static Coroutine GetCustomMaps(string clientVersion, DefinitionType clientType, Action<List<UberstrikeCustomMapView>> callback, Action<Exception> handler) {
			Coroutine result;

			using (MemoryStream memoryStream = new MemoryStream())
			{
				StringProxy.Serialize(memoryStream, clientVersion);
				EnumProxy<DefinitionType>.Serialize(memoryStream, clientType);
				result = MonoInstance.Mono.StartCoroutine(SoapClient.MakeRequest("IApplicationWebServiceContract", "UberStrike.DataCenter.WebService.CWS.ApplicationWebServiceContract.svc", "GetCustomMaps", memoryStream.ToArray(), delegate(byte[] data)
				{
					if (callback != null)
					{
						callback(ListProxy<UberstrikeCustomMapView>.Deserialize(new MemoryStream(data), new ListProxy<UberstrikeCustomMapView>.Deserializer<UberstrikeCustomMapView>(UberstrikeCustomMapViewProxy.Deserialize)));
					}
				}, handler));
			}

			return result;
		}
	}
}
