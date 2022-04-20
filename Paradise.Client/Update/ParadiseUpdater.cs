using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
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

	public static class ParadiseUpdater {
		public static List<UpdateFile> FilesToUpdate = new List<UpdateFile>();

		private static string UpdateDownloadURL => "file:///G:/Documents/Visual%20Studio%202019/Projects/Paradise/release/";
		//private static string UpdatePlatform => "win32";
		private static string UpdateCatalog => "updates.yaml";

		private static UpdatePlatformDefinition CachedUpdates;

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

		public static IEnumerator CheckForUpdates(Action<UpdatePlatformDefinition> updateAvailableCallback, Action errorCallback) {
			FilesToUpdate.Clear();

			if (string.IsNullOrEmpty(ApplicationDataManager_hook.UpdateUrl)) yield break;

			var progressPopup = PopupSystem.ShowProgress(LocalizedStrings.SettingUp, "Checking for updates");

			using (WWW loader = new WWW(string.Join("/", new string[] { ApplicationDataManager_hook.UpdateUrl, UpdateCatalog }))) {
				while (!loader.isDone) {
					progressPopup.Progress = loader.progress;
					yield return null;
				}

				PopupSystem.HideMessage(progressPopup);

				if (!string.IsNullOrEmpty(loader.error)) {
					Debug.LogError($"An error occured while checking for updates: {loader.error}");
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
						yield break;
					}

					CachedUpdates = updateDef;

					foreach (var file in updateDef.files) {
						string path = $"{Application.dataPath}\\{file.localPath}\\{file.filename}";

						if (!File.Exists(path)) {
							FilesToUpdate.Add(file);
							continue;
						} else {
							using (FileStream fileStream = File.OpenRead(path)) {
								if (!string.IsNullOrEmpty(file.md5sum)) {
									using (var md5 = MD5.Create()) {
										var md5sum = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

										if (!md5sum.Equals(file.md5sum)) {
											Debug.Log($"local file {file.filename}: md5 doesn't match (expected {file.md5sum}, got {md5sum})");
											FilesToUpdate.Add(file);
											continue;
										}
									}
								}

								if (!string.IsNullOrEmpty(file.sha256)) {
									using (var sha256 = SHA256.Create()) {
										var sha256sum = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

										if (!sha256sum.Equals(file.sha256)) {
											Debug.Log($"local file {file.filename}: sha256 doesn't match (expected {file.sha256}, got {sha256sum})");
											FilesToUpdate.Add(file);
											continue;
										}
									}
								}

								if (!string.IsNullOrEmpty(file.sha512)) {
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
					PopupSystem.ShowError("Error", $"An error occured while checking for updates:\n{e.Message}", PopupSystem.AlertType.OK);
				}

				if (FilesToUpdate.Count > 0) {
					updateAvailableCallback?.Invoke(CachedUpdates);
				}
			}

			yield break;
		}

		public static IEnumerator InstallUpdates() {
			if (FilesToUpdate.Count == 0) {
				PopupSystem.ShowMessage("Error", "Failed to download updates:\nNo files to update.");
				yield break;
			}

			foreach (var file in FilesToUpdate) {
				var progressPopup = PopupSystem.ShowProgress("Downloading updates", file.filename);

				using (WWW loader = new WWW(string.Join("/", new string[] { file.remoteURL ?? ApplicationDataManager_hook.UpdateUrl, file.remotePath, file.filename }))) {
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

							File.Copy(updateFilename, filename);
						} catch (Exception e) {
							Debug.LogError(e);
						}
					} else {
						PopupSystem.HideMessage(progressPopup);
						PopupSystem.ShowMessage("Error", $"An error occured while downloading updates: {loader.error}", PopupSystem.AlertType.OK);
						yield break;
					}
				}
			}

			PopupSystem.ShowMessage("Update Complete", "Updates have been installed successfully. UberStrike needs to be restarted for completing the installation.", PopupSystem.AlertType.OK, delegate {
				System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike.exe")).WaitForExit();
				Application.Quit();
			});

			yield break;
		}
	}
}
