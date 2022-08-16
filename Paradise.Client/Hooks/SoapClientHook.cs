using HarmonyLib;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UberStrike.WebService.Unity;

namespace Paradise.Client {
	public class SoapClientHook : IParadiseHook {
		public void Hook(Harmony harmonyInstance) {
			var type = typeof(ApplicationWebServiceClient).Assembly.GetType("UberStrike.WebService.Unity.SoapClient");

			var orig_SoapClient_MakeRequest = type.GetMethod("MakeRequest", BindingFlags.Static | BindingFlags.Public);
			var prefix_SoapClient_MakeRequest = typeof(SoapClientHook).GetMethod("MakeRequest_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_SoapClient_MakeRequest, new HarmonyMethod(prefix_SoapClient_MakeRequest), null);
		}

		public static bool MakeRequest_Prefix(string interfaceName, ref string serviceName, string methodName, byte[] data, Action<byte[]> requestCallback, Action<Exception> exceptionHandler) {
			serviceName = Regex.Replace(serviceName, @"^UberStrike\.DataCenter\.WebService\.CWS\.", ApplicationDataManagerHook.WebServicePrefix);
			serviceName = Regex.Replace(serviceName, @"Contract\.svc$", ApplicationDataManagerHook.WebServiceSuffix);

			return true;
		}
	}
}
