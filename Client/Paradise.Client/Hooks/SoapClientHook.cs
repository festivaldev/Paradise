using HarmonyLib;
using log4net;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UberStrike.WebService.Unity;

namespace Paradise.Client {
	public class SoapClientHook : IParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Allows for renaming web service prefixes/suffixes.
		/// </summary>
		public SoapClientHook() { }

		public void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(SoapClientHook)}] hooking {nameof(SoapClient)}");

			var type = typeof(ApplicationWebServiceClient).Assembly.GetType("UberStrike.WebService.Unity.SoapClient");

			var orig_SoapClient_MakeRequest = type.GetMethod("MakeRequest", BindingFlags.Static | BindingFlags.Public);
			var prefix_SoapClient_MakeRequest = typeof(SoapClientHook).GetMethod("MakeRequest_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_SoapClient_MakeRequest, new HarmonyMethod(prefix_SoapClient_MakeRequest), null);
		}

		public static bool MakeRequest_Prefix(string interfaceName, ref string serviceName, string methodName, byte[] data, Action<byte[]> requestCallback, Action<Exception> exceptionHandler) {
			serviceName = Regex.Replace(serviceName, @"^UberStrike\.DataCenter\.WebService\.CWS\.", ParadiseClient.WebServicePrefix);
			serviceName = Regex.Replace(serviceName, @"Contract\.svc$", ParadiseClient.WebServiceSuffix);

			return true;
		}
	}
}
