using HarmonyLib;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UberStrike.WebService.Unity;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Allows for renaming web service prefixes/suffixes.
	/// </summary>
	[HarmonyPatch("UberStrike.WebService.Unity.SoapClient")]
	public class SoapClientHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(SoapClientHook));

		protected static ICryptographyPolicy CryptoPolicy = new CryptographyPolicy();
		private static int RequestId = 0;

		static SoapClientHook() {
			Log.Info($"[{nameof(SoapClientHook)}] hooking {typeof(ApplicationWebServiceClient).Assembly.GetType("UberStrike.WebService.Unity.SoapClient")}");
		}

		[HarmonyPatch("UberStrike.WebService.Unity.SoapClient", "MakeRequest"), HarmonyPrefix]
		public static bool SoapClient_MakeRequest_Prefix(string interfaceName, ref string serviceName, string methodName, ref byte[] data, Action<byte[]> requestCallback, Action<Exception> exceptionHandler) {
			serviceName = Regex.Replace(serviceName, @"^UberStrike\.DataCenter\.WebService\.CWS\.", ParadiseClient.WebServicePrefix);
			serviceName = Regex.Replace(serviceName, @"Contract\.svc$", ParadiseClient.WebServiceSuffix);

			return false;
		}

		[HarmonyPatch("UberStrike.WebService.Unity.SoapClient", "MakeRequest"), HarmonyPostfix]
		public static IEnumerator SoapClient_MakeRequest_Postfix(IEnumerator value, string interfaceName, string serviceName, string methodName, byte[] data, Action<byte[]> requestCallback, Action<Exception> exceptionHandler) {
			if (!string.IsNullOrEmpty(Configuration.EncryptionPassPhrase) && !string.IsNullOrEmpty(Configuration.EncryptionInitVector)) {
				data = CryptoPolicy.RijndaelEncrypt(data, Configuration.EncryptionPassPhrase, Configuration.EncryptionInitVector);
			}

			var requestId = RequestId++;

			var byteArray = Encoding.UTF8.GetBytes($"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><{methodName} xmlns=\"http://tempuri.org/\"><data>{Convert.ToBase64String(data)}</data></{methodName}></s:Body></s:Envelope>");

			var headers = new Dictionary<string, string> {
				{ "SOAPAction", $"\"http://tempuri.org/{interfaceName}/{methodName}\"" },
				{ "Content-type", "text/xml; charset=utf-8" }
			};

			var doc = new XmlDocument();
			var startTime = Time.realtimeSinceStartup;

			LogRequest(requestId, startTime, data.Length, interfaceName, serviceName, methodName);

			yield return new WaitForEndOfFrame();

			if (WebServiceStatistics.IsEnabled) {
				WebServiceStatistics.RecordWebServiceBegin(methodName, byteArray.Length);
			}

			byte[] returnData = null;

			using (WWW request = new WWW(Configuration.WebserviceBaseUrl + serviceName, byteArray, headers)) {
				yield return request;

				if (WebServiceStatistics.IsEnabled) {
					WebServiceStatistics.RecordWebServiceEnd(methodName, request.bytes.Length, request.isDone && string.IsNullOrEmpty(request.error));
				}

				try {
					if (Configuration.SimulateWebservicesFail) {
						throw new Exception($"Simulated Webservice fail when calling {interfaceName}/{methodName}");
					}

					if (!request.isDone || !string.IsNullOrEmpty(request.error)) {
						LogResponse(requestId, Time.realtimeSinceStartup, request.error, Time.time - startTime, 0);

						throw new Exception(string.Concat(new string[] {
							request.error,
							"\nWWW Url: ",
							Configuration.WebserviceBaseUrl,
							"\nService: ",
							serviceName,
							"\nMethod: ",
							methodName
						}));
					}

					if (!string.IsNullOrEmpty(request.text)) {
						try {
							doc.LoadXml(request.text);

							XmlNodeList result = doc.GetElementsByTagName(methodName + "Result");

							if (result.Count <= 0) {
								LogResponse(requestId, Time.realtimeSinceStartup, request.text, Time.time - startTime, 0);

								throw new Exception($"WWW Request to {Configuration.WebserviceBaseUrl}{serviceName} failed with content {request.text}");
							}

							returnData = Convert.FromBase64String(result[0].InnerXml);

							if (returnData.Length == 0) {
								LogResponse(requestId, Time.realtimeSinceStartup, request.text, Time.time - startTime, 0);

								throw new Exception($"WWW Request to {Configuration.WebserviceBaseUrl}{serviceName} failed with content {request.text}");
							}

							LogResponse(requestId, Time.realtimeSinceStartup, "OK", Time.realtimeSinceStartup - startTime, request.bytes.Length);
						} catch {
							LogResponse(requestId, Time.time, request.text, Time.realtimeSinceStartup - startTime, 0);

							throw new Exception($"Error reading XML return for method call {interfaceName}/{methodName}: {request.text}");
						}
					}

					if (requestCallback != null) {
						if (!string.IsNullOrEmpty(Configuration.EncryptionPassPhrase) && !string.IsNullOrEmpty(Configuration.EncryptionInitVector)) {
							requestCallback(CryptoPolicy.RijndaelDecrypt(returnData, Configuration.EncryptionPassPhrase, Configuration.EncryptionInitVector));
						} else {
							requestCallback(returnData);
						}
					}
				} catch (Exception ex) {
					if (exceptionHandler != null) {
						exceptionHandler(ex);
					} else {
						Debug.LogError("SoapClient Unhandled Exception: " + ex.Message + "\n" + ex.StackTrace);
					}
				}
			}

			yield break;
		}

		private static void LogRequest(int id, float time, int sizeBytes, string interfaceName, string serviceName, string methodName) {
			AccessTools.TypeByName("UberStrike.WebService.Unity.SoapClient").GetMethod("LogRequest", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { id, time, sizeBytes, interfaceName, serviceName, methodName });

			string text = ((float)sizeBytes / 1000f).ToString();
			Log.Debug($"[REQ] ID:{id} Time:{time:N2} Size:{text:N2}Kb Service:{serviceName} Interface:{interfaceName} Method:{methodName}");
		}

		private static void LogResponse(int id, float time, string message, float duration, int sizeBytes) {
			AccessTools.TypeByName("UberStrike.WebService.Unity.SoapClient").GetMethod("LogResponse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { id, time, message, duration, sizeBytes });

			string text = ((float)sizeBytes / 1000f).ToString();
			Log.Debug($"[RSP] ID:{id} Time:{time:N2} Size:{text:N2}Kb Duration:{duration:N2}s Status:{message}");
		}
	}
}
