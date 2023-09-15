using HarmonyLib;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace Paradise.Client {
	[XmlRoot]
	public class ScreenResolutionOptions {
		[XmlArray]
		[XmlArrayItem(typeof(ScreenResolution))]
		public List<ScreenResolution> ScreenResolutionList { get; set; }
	}

#pragma warning disable IDE1006
	public class ScreenResolution {
		[XmlAttribute]
		public int width { get; set; }

		[XmlAttribute]
		public int height { get; set; }

		[XmlAttribute]
		public float refreshRate { get; set; }
	}
#pragma warning restore IDE1006

	/// <summary>
	/// Adds all available screen resolutions to the Video settings pane if they're missing.
	/// </summary>
	[HarmonyPatch(typeof(ScreenResolutionManager))]
	public class ScreenResolutionManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ScreenResolutionManagerHook));

		static ScreenResolutionManagerHook() {
			Log.Info($"[{nameof(ScreenResolutionManagerHook)}] hooking {nameof(ScreenResolutionManager)}");
		}

		[HarmonyPrepare]
		public static void Prepare() {
			XmlSerializer serializer = new XmlSerializer(typeof(ScreenResolutionOptions));
			using (Stream stream = Assembly.GetAssembly(typeof(ParadiseClient)).GetManifestResourceStream("Paradise.Client.ScreenResolutions.xml")) {
				using (StreamReader reader = new StreamReader(stream)) {
					ScreenResolutionManager.Resolutions.Clear();

					var screenResolutionOptions = (ScreenResolutionOptions)serializer.Deserialize(reader);
					foreach (var resolution in screenResolutionOptions.ScreenResolutionList) {
						if (Screen.currentResolution.width >= resolution.width && Screen.currentResolution.height >= resolution.height) {
							ScreenResolutionManager.Resolutions.Add(new Resolution {
								width = resolution.width,
								height = resolution.height
							});
						}
					}
				}
			}

			if (ScreenResolutionManager.Resolutions.Find(_ => _.width == Screen.currentResolution.width && _.height == Screen.currentResolution.height).Equals(default(Resolution))) {
				ScreenResolutionManager.Resolutions.Add(Screen.currentResolution);
			}

			ScreenResolutionManager.Resolutions.Sort((a, b) => {
				var result = a.width.CompareTo(b.width);

				return result != 0 ? result : a.height.CompareTo(b.height);
			});
		}
	}
}
