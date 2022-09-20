using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Paradise.Client {
#pragma warning disable IDE1006
	internal class UpdateCatalog {
		public List<UpdatePlatformDefinition> platforms { get; set; }
	}

	internal class UpdatePlatformDefinition {
		public string platform { get; set; }
		public string version { get; set; }
		public string build { get; set; }
		public List<UpdateFile> files { get; set; }
		public List<UpdateFile> removedFiles { get; set; }
	}

	internal class UpdateFile {
		public string filename { get; set; }
		public string description { get; set; }
		public string localPath { get; set; }
		public string remoteURL { get; set; }
		public string remotePath { get; set; }
		public uint filesize { get; set; }
		public string md5sum { get; set; }
		public string sha256 { get; set; }
		public string sha512 { get; set; }
	}
#pragma warning restore IDE1006

	internal class ParadiseUpdater : MonoBehaviour {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseUpdater));

		private DateTime LastUpdateCheckTimeStamp = DateTime.MinValue;

		private UpdatePlatformDefinition CachedUpdates;
		private readonly List<UpdateFile> FilesToUpdate = new List<UpdateFile>();
		private readonly List<UpdateFile> FilesToRemove = new List<UpdateFile>();

		private static string UpdateCatalog => $"updates-{ParadiseClient.UpdateChannel.ToString().ToLower()}.yaml";
		private static string UpdatePlatform {
			get {
				switch (Application.platform) {
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
					case RuntimePlatform.WindowsWebPlayer:
						return "win32";
					case RuntimePlatform.OSXPlayer:
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXWebPlayer:
						return "darwin32";
					default: break;
				}

				return null;
			}
		}

		private ProgressPopupDialog _progressPopup;

		public IEnumerator CheckForUpdatesIfNecessary(Action<UpdatePlatformDefinition> updateAvailableCallback, Action<string> errorCallback) {
			if (Math.Abs((LastUpdateCheckTimeStamp - DateTime.Now).TotalHours) < 1) {
				yield break;
			}

			LastUpdateCheckTimeStamp = DateTime.Now;

			if (!ParadiseClient.AutoUpdates) {
				Log.Info("Automatic updates disabled");

				if (PlayerDataManager.AccessLevel < MemberAccessLevel.Admin) {
					PopupSystem.ShowMessage("Automatic Updates disabled", "You have disabled automatic updates. If you want to play UberStrike, you may need to update the Paradise runtime.\n\nPlease enable automatic updates in order to receive updates.", PopupSystem.AlertType.OK);
				}

				yield break;
			}

			FilesToUpdate.Clear();

			if (string.IsNullOrEmpty(ParadiseClient.UpdateUrl)) yield break;

			var updateUri = string.Join("", new string[] { ParadiseClient.UpdateUrl, UpdateCatalog });

			_progressPopup = PopupSystem.ShowProgress(LocalizedStrings.SettingUp, "Checking for updates...");

			UnityRuntime.StartRoutine(DownloadUpdateCatalog(updateUri, delegate (UpdateCatalog updateCatalog) {
				OnUpdateCatalogDownloadComplete(updateCatalog, updateAvailableCallback, errorCallback);
			}, delegate (HttpResponse responseHeader) {
				switch (responseHeader.StatusCode) {
					case HttpStatusCode.NotFound:
						Log.Info("Downloading update catalog resulted in 404, retrying using fallback URI");
						updateUri = string.Join("", new string[] { ParadiseClient.UpdateUrl, "updates.yaml" });

						UnityRuntime.StartRoutine(DownloadUpdateCatalog(updateUri, delegate (UpdateCatalog updateCatalog) {
							OnUpdateCatalogDownloadComplete(updateCatalog, updateAvailableCallback, errorCallback);
						}, delegate (HttpResponse _responseHeader) {
							PopupSystem.HideMessage(_progressPopup);

							errorCallback?.Invoke($"Failed to download update catalog.\n{responseHeader.StatusText}");
						}));

						break;
				}
			}));

			yield break;
		}

		private IEnumerator DownloadUpdateCatalog(string updateUri, Action<UpdateCatalog> successCallback, Action<HttpResponse> errorCallback) {
			Log.Info($"Attempting to download update catalog from {updateUri}");

			using (WWW loader = new WWW(updateUri)) {
				while (!loader.isDone) {
					_progressPopup.Progress = loader.progress;
					yield return null;
				}

				PopupSystem.HideMessage(_progressPopup);

				var responseHeader = HTTPStatusParser.ParseHeader(loader.responseHeaders["STATUS"]);

				if (responseHeader.StatusCode != HttpStatusCode.OK) {
					errorCallback?.Invoke(responseHeader);
					yield break;
				}

				try {
					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(CamelCaseNamingConvention.Instance)
						.Build();

					var updateCatalog = deserializer.Deserialize<UpdateCatalog>(loader.text);

					successCallback?.Invoke(updateCatalog);
				} catch (Exception e) {
					Log.Error($"An error occured while checking for updates:\n{e.Message}");
					Log.Debug(e);

					yield break;
				}
			}

			yield break;
		}

		private void OnUpdateCatalogDownloadComplete(UpdateCatalog updateCatalog, Action<UpdatePlatformDefinition> updateAvailableCallback, Action<string> errorCallback) {
			var universalUpdates = updateCatalog.platforms.Find(_ => _.platform == "universal");

			if (universalUpdates == null) {
				Log.Error("Update catalog does not contain universal platform.");
			} else {
				if (universalUpdates.files != null) {
					CheckUpdatedFiles(universalUpdates, errorCallback);
				}

				if (universalUpdates.removedFiles != null) {
					CheckRemovedFiles(universalUpdates);
				}
			}

			var platformUpdates = updateCatalog.platforms.Find(_ => _.platform == UpdatePlatform);

			if (platformUpdates == null) {
				Log.Error($"Update catalog does not contain platform \"{UpdatePlatform}\".");
			} else {
				if (platformUpdates.files != null) {
					CheckUpdatedFiles(platformUpdates, errorCallback);
				}

				if (platformUpdates.removedFiles != null) {
					CheckRemovedFiles(platformUpdates);
				}
			}

			CachedUpdates = universalUpdates ?? platformUpdates;

			if (FilesToUpdate.Count > 0 || FilesToRemove.Count > 0) {
				Log.Info($"Update available: {CachedUpdates.version} (Build {CachedUpdates.build}), files to update: {FilesToUpdate.Count}; files to remove: {FilesToRemove.Count}");
				updateAvailableCallback?.Invoke(CachedUpdates);
			} else {
				Log.Info("No update available.");
			}
		}

		private void CheckUpdatedFiles(UpdatePlatformDefinition updateDef, Action<string> errorCallback) {
			if (updateDef.files.Count == 0) {
				Log.Info($"Update catalog doesn't contain any files for platform \"{updateDef.platform}\", aborting...");
				return;
			}

			foreach (var file in updateDef.files) {
				string path = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";

				if (!File.Exists(path)) {
					Log.Info($"Local file {file.filename} doesn't exist");
					FilesToUpdate.Add(file);
					continue;
				} else {
					try {
						if (!string.IsNullOrEmpty(file.md5sum)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var md5 = MD5.Create()) {
									var md5sum = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!md5sum.Equals(file.md5sum)) {
										Log.Info($"Local file {file.filename}: MD5 doesn't match (expected {file.md5sum}, got {md5sum})");
										FilesToUpdate.Add(file);
										continue;
									}
								}
							}
						}

						if (!string.IsNullOrEmpty(file.sha256)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var sha256 = SHA256.Create()) {
									var sha256sum = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!sha256sum.Equals(file.sha256)) {
										Log.Info($"Local file {file.filename}: SHA256 doesn't match (expected {file.sha256}, got {sha256sum})");
										FilesToUpdate.Add(file);
										continue;
									}
								}
							}
						}

						if (!string.IsNullOrEmpty(file.sha512)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var sha512 = SHA512.Create()) {
									var sha512sum = BitConverter.ToString(sha512.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!sha512sum.Equals(file.sha512)) {
										Log.Info($"Local file {file.filename}: SHA512 doesn't match (expected {file.sha512}, got {sha512sum})");
										FilesToUpdate.Add(file);
										continue;
									}
								}
							}
						}
					} catch (Exception e) {
						errorCallback?.Invoke($"An error occured while checking for updates:\n{e.Message}");
						Log.Debug(e);
						return;
					}
				}
			}
		}

		private void CheckRemovedFiles(UpdatePlatformDefinition updateDef) {
			foreach (var file in updateDef.removedFiles) {
				string path = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";

				if (File.Exists(path)) {
					Log.Info($"Local file {file.filename} exists, marked for removal");
					FilesToRemove.Add(file);
				}
			}
		}

		public IEnumerator InstallUpdates(Action updateCompleteCallback, Action<string> errorCallback) {
			if (FilesToUpdate.Count == 0) {
				errorCallback?.Invoke("Failed to download updates:\nNo files to update.");

				yield break;
			}

			foreach (var file in FilesToUpdate) {
				_progressPopup = PopupSystem.ShowProgress($"Downloading updates... ({FilesToUpdate.IndexOf(file) + 1}/{FilesToUpdate.Count})", $"{file.filename} ({FormatBytes(file.filesize)})");

				using (WWW loader = new WWW(string.Join("/", new string[] { file.remoteURL ?? ParadiseClient.UpdateUrl, file.remotePath, file.filename }))) {
					Log.Info($"Downloading remote file: {loader.url}");

					while (!loader.isDone) {
						_progressPopup.Progress = loader.progress;
						yield return null;
					}

					PopupSystem.HideMessage(_progressPopup);

					var responseHeader = HTTPStatusParser.ParseHeader(loader.responseHeaders["STATUS"]);

					if (responseHeader.StatusCode != HttpStatusCode.OK) {
						errorCallback?.Invoke($"Failed to download {loader.url} {file.filename}:\n{responseHeader.StatusText}");
						yield break;
					}

					try {
						if (!Directory.Exists($"{Application.dataPath}\\Updates")) {
							Directory.CreateDirectory($"{Application.dataPath}\\Updates");
						}

						if (!string.IsNullOrEmpty(file.md5sum)) {
							using (var md5 = MD5.Create()) {
								var md5sum = BitConverter.ToString(md5.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!md5sum.Equals(file.md5sum)) {
									errorCallback?.Invoke($"{file.filename}: Hash sum mismatch. Please contact your server administrator.");
									Log.Error($"Downloaded file {file.filename}: MD5 doesn't match (expected {file.md5sum}, got {md5sum})");
									continue;
								}
							}
						}

						if (!string.IsNullOrEmpty(file.sha256)) {
							using (var sha256 = SHA256.Create()) {
								var sha256sum = BitConverter.ToString(sha256.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!sha256sum.Equals(file.sha256)) {
									errorCallback?.Invoke($"{file.filename}: Hash sum mismatch. Please contact your server administrator.");
									Debug.LogError($"Downloaded file {file.filename}: SHA256 doesn't match (expected {file.sha256}, got {sha256sum})");
									continue;
								}
							}
						}

						if (!string.IsNullOrEmpty(file.sha512)) {
							using (var sha512 = SHA512.Create()) {
								var sha512sum = BitConverter.ToString(sha512.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!sha512sum.Equals(file.sha512)) {
									errorCallback?.Invoke($"{file.filename}: Hash sum mismatch. Please contact your server administrator.");
									Debug.LogError($"Downloaded file {file.filename}: SHA512 doesn't match (expected {file.sha512}, got {sha512sum})");
									continue;
								}
							}
						}
					} catch (Exception e) {
						PopupSystem.HideMessage(_progressPopup);

						errorCallback?.Invoke($"An error occured while downloading updates: {e.Message}");
						Log.Debug(e);

						yield break;
					}

					try {
						var tempFilename = $"{Application.dataPath}\\Updates\\{file.filename}";
						File.WriteAllBytes(tempFilename, loader.bytes);

						if (!Directory.Exists($"{Application.dataPath}\\{file.localPath}")) {
							Directory.CreateDirectory($"{Application.dataPath}\\{file.localPath}");
						}

						var filename = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";
						if (File.Exists(filename)) {
							// File will be removed on game restart

							File.Move(filename, filename + ".bak");
						}

						// Disabled for testing
						File.Move(tempFilename, filename);
					} catch (Exception e) {
						PopupSystem.HideMessage(_progressPopup);

						errorCallback?.Invoke($"An error occured while installing updates: {e.Message}");
						Log.Debug(e);

						yield break;
					}
				}
			}

			foreach (var file in FilesToRemove) {
				try {
					if (!Directory.Exists($"{Application.dataPath}\\{file.localPath}")) {
						continue;
					}

					var filename = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";

					if (File.Exists(filename)) {
						// File will be removed on game restart

						File.Move(filename, filename + ".bak");
					}
				} catch (Exception e) {
					PopupSystem.HideMessage(_progressPopup);

					errorCallback?.Invoke($"An error occured while installing updates: {e.Message}");
					Log.Debug(e);

					yield break;
				}
			}

			updateCompleteCallback?.Invoke();
		}

		public static IEnumerator CleanupUpdates() {
			Log.Info("Cleaning cached update files");

			// Delete backup files in UberStrike_Data\Managed
			if (Directory.Exists(string.Join("/", new string[] { Application.dataPath, "Managed" }))) {
				var files = Directory.GetFiles(string.Join("/", new string[] { Application.dataPath, "Managed" }), "*.bak");

				foreach (var file in files) {
					File.Delete(file);
				}
			}

			// Delete backup files in UberStrike_Data\Maps
			if (Directory.Exists(string.Join("/", new string[] { Application.dataPath, "Maps" }))) {
				var files = Directory.GetFiles(string.Join("/", new string[] { Application.dataPath, "Maps" }), "*.bak");

				foreach (var file in files) {
					File.Delete(file);
				}
			}

			// Delete files UberStrike_Data\Updates
			if (Directory.Exists(string.Join("/", new string[] { Application.dataPath, "Updates" }))) {
				var files = Directory.GetFiles(string.Join("/", new string[] { Application.dataPath, "Updates" }), "*");

				foreach (var file in files) {
					File.Delete(file);
				}
			}

			yield break;
		}

		static string FormatBytes(Int64 value, int decimalPlaces = 1) {
			string[] SizeSuffixes =
				   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

			if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
			if (value < 0) { return "-" + FormatBytes(-value, decimalPlaces); }
			if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

			// mag is 0 for bytes, 1 for KB, 2, for MB, etc.
			int mag = (int)Math.Log(value, 1024);

			// 1L << (mag * 10) == 2 ^ (10 * mag) 
			// [i.e. the number of bytes in the unit corresponding to mag]
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			// make adjustment when the value is large enough that
			// it would round up to 1000 or more
			if (Math.Round(adjustedSize, decimalPlaces) >= 1000) {
				mag += 1;
				adjustedSize /= 1024;
			}

			return string.Format("{0:n" + decimalPlaces + "} {1}",
				adjustedSize,
				SizeSuffixes[mag]);
		}
	}
}
