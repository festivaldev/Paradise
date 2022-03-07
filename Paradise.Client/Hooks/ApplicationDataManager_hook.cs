using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Paradise.Client {
	public class ApplicationDataManager_hook {
		public static string WebServiceBaseUrl { get; private set; } = "https://ws.uberstrike.com/2.0/";
		public static string ImagePath { get; private set; } = "https://static.uberstrike.com/images/";

		static ApplicationDataManager_hook() {
			XmlSerializer ser = new XmlSerializer(typeof(ParadiseSettings));

			using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike_Data\\ParadiseSettings.xml")))) {
				try {
					var settings = (ParadiseSettings)ser.Deserialize(reader);

					WebServiceBaseUrl = ForceTrailingSlash(settings.WebServiceBaseUrl);
					ImagePath = ForceTrailingSlash(settings.ImagePath);
				} catch (Exception e) {
					Debug.LogError(e.Message);
				}
			}
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
