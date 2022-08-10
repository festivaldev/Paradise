using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Paradise.Client {
	public class UpdateCatalog {
		public List<UpdatePlatformDefinition> platforms { get; set; }
	}

	public class UpdatePlatformDefinition {
		public string platform { get; set; }
		public string version { get; set; }
		public string build { get; set; }
		public List<UpdateFile> files { get; set; }
	}

	public class UpdateFile {
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

	public class ParadiseUpdater : MonoBehaviour {
		private bool DidCheckForUpdates;
		private DateTime LastUpdateCheckTimeStamp;

		private UpdatePlatformDefinition CachedUpdates;
		private List<UpdateFile> FilesToUpdate = new List<UpdateFile>();

		private static string UpdateCatalog => $"updates-{ApplicationDataManagerHook.UpdateChannel}.yaml";
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

		public IEnumerator CheckForUpdatesIfNecessary(Action<UpdatePlatformDefinition> updateAvailableCallback, Action<string> errorCallback) {
			if (DidCheckForUpdates) {
				if ((LastUpdateCheckTimeStamp - DateTime.Now).TotalHours < 1) {
					yield break;
				}
			}

			DidCheckForUpdates = true;
			LastUpdateCheckTimeStamp = DateTime.UtcNow;

			if (!ApplicationDataManagerHook.AutoUpdates) {
				PopupSystem.ShowMessage("Automatic Updates disabled", "You have disabled automatic updates. If you want to play UberStrike, you may need to update the Paradise runtime.\n\nPlease enable automatic updates in order to receive updates.", PopupSystem.AlertType.OK);
				yield break;
			}

			FilesToUpdate.Clear();

			if (string.IsNullOrEmpty(ApplicationDataManagerHook.UpdateUrl)) yield break;

			var progressPopup = PopupSystem.ShowProgress(LocalizedStrings.SettingUp, "Checking for updates");

			using (WWW loader = new WWW(string.Join("/", new string[] { ApplicationDataManagerHook.UpdateUrl, UpdateCatalog }))) {
				while (!loader.isDone) {
					progressPopup.Progress = loader.progress;
					yield return null;
				}

				PopupSystem.HideMessage(progressPopup);

				if (!string.IsNullOrEmpty(loader.error)) {
					errorCallback?.Invoke($"An error occured while checking for updates: {loader.error}");

					yield break;
				}

				try {
					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(CamelCaseNamingConvention.Instance)
						.Build();

					var updateCatalog = deserializer.Deserialize<UpdateCatalog>(loader.text);
					var updateDef = updateCatalog.platforms.Find(_ => _.platform == UpdatePlatform);

					if (updateDef == null) {
						Debug.LogError($"Update catalog does not contain platform \"{UpdatePlatform}\"");

						//errorCallback?.Invoke($"Update catalog does not contain platform \"{UpdatePlatform}\"");

						yield break;
					}

					CachedUpdates = updateDef;

					if (updateDef.files.Count == 0) {
						Debug.Log($"Update catalog doesn't contain any files for platform \"{UpdatePlatform}\", aborting...");
						yield break;
					}

					foreach (var file in updateDef.files) {
						string path = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";

						if (!File.Exists(path)) {
							FilesToUpdate.Add(file);
							continue;
						} else {
							if (!string.IsNullOrEmpty(file.md5sum)) {
								using (FileStream fileStream = File.OpenRead(path)) {
									using (var md5 = MD5.Create()) {
										var md5sum = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

										if (!md5sum.Equals(file.md5sum)) {
											Debug.Log($"local file {file.filename}: md5 doesn't match (expected {file.md5sum}, got {md5sum})");
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
											Debug.Log($"local file {file.filename}: sha256 doesn't match (expected {file.sha256}, got {sha256sum})");
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
											Debug.Log($"local file {file.filename}: sha512 doesn't match (expected {file.sha512}, got {sha512sum})");
											FilesToUpdate.Add(file);
											continue;
										}
									}
								}
							}
						}
					}
				} catch (Exception e) {
					errorCallback?.Invoke($"An error occured while checking for updates:\n{e.Message}");

					yield break;
				}

				if (FilesToUpdate.Count > 0) {
					updateAvailableCallback?.Invoke(CachedUpdates);
				}
			}

			yield break;
		}

		public IEnumerator InstallUpdates(Action updateCompleteCallback, Action<string> errorCallback) {
			if (FilesToUpdate.Count == 0) {
				errorCallback?.Invoke("Failed to download updates:\nNo files to update.");

				yield break;
			}

			foreach (var file in FilesToUpdate) {
				var progressPopup = PopupSystem.ShowProgress("Downloading updates", file.filename);

				using (WWW loader = new WWW(string.Join("/", new string[] { file.remoteURL ?? ApplicationDataManagerHook.UpdateUrl, file.remotePath, file.filename }))) {
					while (!loader.isDone) {
						progressPopup.Progress = loader.progress;
						yield return null;
					}

					if (string.IsNullOrEmpty(loader.error)) {
						PopupSystem.HideMessage(progressPopup);

						try {
							if (!Directory.Exists($"{Application.dataPath}\\Updates")) {
								Directory.CreateDirectory($"{Application.dataPath}\\Updates");
							}

							if (!string.IsNullOrEmpty(file.md5sum)) {
								using (var md5 = MD5.Create()) {
									var md5sum = BitConverter.ToString(md5.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

									if (!md5sum.Equals(file.md5sum)) {
										Debug.LogError($"downloaded file {file.filename}: md5 doesn't match (expected {file.md5sum}, got {md5sum})");
										continue;
									}
								}
							}

							if (!string.IsNullOrEmpty(file.sha256)) {
								using (var sha256 = SHA256.Create()) {
									var sha256sum = BitConverter.ToString(sha256.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

									if (!sha256sum.Equals(file.sha256)) {
										Debug.LogError($"downloaded file {file.filename}: sha256 doesn't match (expected {file.sha256}, got {sha256sum})");
										continue;
									}
								}
							}

							if (!string.IsNullOrEmpty(file.sha512)) {
								using (var sha512 = SHA512.Create()) {
									var sha512sum = BitConverter.ToString(sha512.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

									if (!sha512sum.Equals(file.sha512)) {
										Debug.LogError($"downloaded file {file.filename}: sha512 doesn't match (expected {file.sha512}, got {sha512sum})");
										continue;
									}
								}
							}

							var updateFilename = $"{Application.dataPath}\\Updates\\{file.filename}";
							File.WriteAllBytes(updateFilename, loader.bytes);

							if (!Directory.Exists($"{Application.dataPath}\\{file.localPath}")) {
								Directory.CreateDirectory($"{Application.dataPath}\\{file.localPath}");
							}

							var filename = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";
							if (File.Exists(filename)) {
								File.Move(filename, filename + ".bak");
								File.Delete(filename + ".bak");
							}

							File.Move(updateFilename, filename);
						} catch (Exception e) {
							Debug.LogError(e);
						}
					} else {
						PopupSystem.HideMessage(progressPopup);

						errorCallback?.Invoke($"An error occured while downloading updates: {loader.error}");

						yield break;
					}
				}
			}

			updateCompleteCallback?.Invoke();

			yield break;
		}
	}
}
