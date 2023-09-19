using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Paradise.Client {
	public enum UpdateChannel {
		Stable,
		Beta
	}

	internal class UpdateCatalog {
		[YamlMember(Alias = "version")]
		public string Version { get; set; }

		[YamlMember(Alias = "build")]
		public string Build { get; set; }

		[YamlMember(Alias = "channel")]
		public UpdateChannel Channel { get; set; }

		[YamlMember(Alias = "platforms")]
		public Dictionary<string, UpdatePlatformDefinition> Platforms { get; set; }
	}

	internal class UpdatePlatformDefinition {
		[YamlMember(Alias = "platform")]
		public string Platform { get; set; }

		[YamlMember(Alias = "files")]
		public List<UpdateFile> Files { get; set; }

		[YamlMember(Alias = "removedFiles")]
		public List<UpdateFile> RemovedFiles { get; set; }
	}

	internal class UpdateFile {
		[YamlMember(Alias = "filename")]
		public string FileName { get; set; }

		[YamlMember(Alias = "description")]
		public string Description { get; set; }

		[YamlMember(Alias = "localPath")]
		public string LocalPath { get; set; }

		[YamlMember(Alias = "remoteURL")]
		public string RemoteURL { get; set; }

		[YamlMember(Alias = "remotePath")]
		public string RemotePath { get; set; }

		[YamlMember(Alias = "filesize")]
		public long FileSize { get; set; }

		[YamlMember(Alias = "optional")]
		public bool IsOptional { get; set; }

		[YamlMember(Alias = "md5sum")]
		public string MD5 { get; set; }

		[YamlMember(Alias = "sha256")]
		public string SHA256 { get; set; }

		[YamlMember(Alias = "sha512")]
		public string SHA512 { get; set; }
	}

	internal class ParadiseUpdater : AutoMonoBehaviour<ParadiseUpdater> {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseUpdater));

		private DateTime LastUpdateCheckTimeStamp = DateTime.MinValue;

		private UpdateCatalog CachedUpdates;
		private readonly List<UpdateFile> FilesToUpdate = new List<UpdateFile>();
		private readonly List<UpdateFile> FilesToRemove = new List<UpdateFile>();

		private static string UpdateCatalogUrl => $"{ParadiseClient.Settings.FileServerUrl}/{ParadiseClient.Settings.UpdateEndpoint}/v2/";
		private static string UpdateCatalog => $"{ParadiseClient.Settings.UpdateChannel.ToString().ToLower()}/updates.yml";
		private static string UpdatePlatform {
			get {
				switch (Application.platform) {
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
					case RuntimePlatform.WindowsWebPlayer:
						return "win";
					case RuntimePlatform.OSXPlayer:
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXWebPlayer:
						return "darwin";
					default: break;
				}

				return null;
			}
		}

		private static bool CanIgnoreUpdates => !PlayerDataManager.IsPlayerLoggedIn || PlayerDataManager.AccessLevel >= MemberAccessLevel.SeniorQA;

		private ProgressPopupDialog progressPopup;

		public IEnumerator CheckForUpdatesIfNecessary(Action<UpdateCatalog> catalogDownloadedCallback, Action<string> errorCallback) {
			if (Math.Abs((LastUpdateCheckTimeStamp - DateTime.Now).TotalHours) < 1) {
				yield break;
			}

			LastUpdateCheckTimeStamp = DateTime.Now;

			if (!ParadiseClient.Settings.AutoUpdates) {
				Log.Info("Automatic updates disabled");

				if (!CanIgnoreUpdates) {
					PopupSystem.ShowMessage("Automatic Updates disabled", "You have disabled automatic updates. If you want to play UberStrike, you may need to update the Paradise runtime.\n\nPlease enable automatic updates in order to receive updates.", PopupSystem.AlertType.OK, () => {
						catalogDownloadedCallback?.Invoke(null);
					});
				} else {
					catalogDownloadedCallback?.Invoke(null);
				}

				yield break;
			}

			FilesToUpdate.Clear();
			FilesToRemove.Clear();

			var updateUri = string.Join("", new string[] { UpdateCatalogUrl, UpdateCatalog });

			progressPopup = PopupSystem.ShowProgress(LocalizedStrings.SettingUp, "Checking for updates...");

			UnityRuntime.StartRoutine(DownloadUpdateCatalog(updateUri, delegate (UpdateCatalog updateCatalog) {
				UnityRuntime.StartRoutine(OnUpdateCatalogDownloadComplete(updateCatalog, catalogDownloadedCallback, errorCallback));
			}, delegate (HttpResponse responseHeader) {
				errorCallback?.Invoke($"Failed to download update catalog.\n{responseHeader.StatusText}");
			}));

			yield break;
		}

		public static IEnumerator CleanupUpdates() {
			Log.Info("Cleaning cached update files");

			// Delete downloaded update files
			if (Directory.Exists(string.Join("/", new string[] { Application.dataPath, "Updates" }))) {
				var files = Directory.GetFiles(string.Join("/", new string[] { Application.dataPath, "Updates" }), "*");

				foreach (var file in files) {
					File.Delete(file);
				}
			}

			// Delete backup files
			if (Directory.Exists(string.Join("/", new string[] { Application.dataPath, "Updates", "bak" }))) {
				Directory.Delete(string.Join("/", new string[] { Application.dataPath, "Updates", "bak" }), true);
			}

			yield break;
		}

		public static void HandleUpdateAvailable(UpdateCatalog updateCatalog) {
			HandleUpdateAvailable(updateCatalog, null);
		}

		public static void HandleUpdateAvailable(UpdateCatalog updateCatalog, Action ignoreCallback) {
			PopupSystem.ShowMessage("Update available", $"A mandatory update is available. You need to install this update in order to play.\n\nVersion:{updateCatalog.Version ?? "Unknown"} ({updateCatalog.Build ?? "Unknown"})\nChannel: {updateCatalog.Channel.ToString() ?? "Unknown"}\n\nIf you choose to ignore this update, you will be asked again the next time you launch UberStrike.", PopupSystem.AlertType.OKCancel, delegate {
				UnityRuntime.StartRoutine(Instance.InstallUpdates(HandleUpdateComplete, HandleUpdateError));
			}, "Update", delegate {
				if (!CanIgnoreUpdates) {
					Application.Quit();
				} else {
					ignoreCallback?.Invoke();
				}
			}, CanIgnoreUpdates ? "Ignore" : "Quit");
		}

		public static void HandleUpdateComplete() {
			PopupSystem.ShowMessage("Update Complete", "Updates have been installed successfully. In order to complete the installation, UberStrike needs to be restarted.", PopupSystem.AlertType.OK, delegate {
				System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike.exe")).WaitForExit(1000);
				Application.Quit();
			});
		}

		public static void HandleUpdateError(string message) {
			HandleUpdateError(message, null);
		}

		public static void HandleUpdateError(string message, Action callback = null) {
			Log.Error(message);
			PopupSystem.ShowMessage("Error", message, PopupSystem.AlertType.OK, () => {
				callback?.Invoke();
			});
		}



		private IEnumerator DownloadUpdateCatalog(string updateUri, Action<UpdateCatalog> successCallback, Action<HttpResponse> errorCallback) {
			Log.Info($"Attempting to download update catalog from {updateUri}");

			using (WWW loader = new WWW(updateUri)) {
				while (!loader.isDone) {
					yield return null;
				}

				var responseHeader = HTTPStatusParser.ParseHeader(loader.responseHeaders["STATUS"]);

				if (responseHeader.StatusCode != HttpStatusCode.OK) {
					errorCallback?.Invoke(responseHeader);
					yield break;
				}

				progressPopup.Progress = 0.3f;
				Log.Info("Successfully downloaded update catalog");

				yield return new WaitForSeconds(0.5f);

				try {
					var deserializer = new DeserializerBuilder()
						.WithNamingConvention(CamelCaseNamingConvention.Instance)
						.Build();

					var updateCatalog = deserializer.Deserialize<UpdateCatalog>(loader.text);

					Log.Info("Successfully parsed update catalog");
					progressPopup.Progress = 0.6f;

					successCallback?.Invoke(updateCatalog);
				} catch (Exception e) {
					Log.Error($"An error occured while checking for updates:\n{e.Message}");
					Log.Debug(e);

					yield break;
				}
			}

			yield break;
		}

		private IEnumerator OnUpdateCatalogDownloadComplete(UpdateCatalog updateCatalog, Action<UpdateCatalog> updateAvailableCallback, Action<string> errorCallback) {
			yield return new WaitForSeconds(1f);

			CachedUpdates = updateCatalog;

			var universalUpdates = updateCatalog.Platforms["universal"];
			var platformUpdates = updateCatalog.Platforms[UpdatePlatform];

			var totalFiles = (universalUpdates?.Files.Count ?? 0) + (platformUpdates?.Files.Count ?? 0);

			new Thread(new ThreadStart(() => {
				progressPopup.Text = "Checking files for updates...";

				if (universalUpdates == null) {
					Log.Error("Update catalog does not contain universal platform.");
				} else {
					if (universalUpdates.Files != null) {
						CheckUpdatedFiles(universalUpdates, 0, totalFiles, errorCallback);
					}

					if (universalUpdates.RemovedFiles != null) {
						CheckRemovedFiles(universalUpdates);
					}
				}

				if (platformUpdates == null) {
					Log.Error($"Update catalog does not contain platform \"{UpdatePlatform}\".");
				} else {
					if (platformUpdates.Files != null) {
						CheckUpdatedFiles(platformUpdates, universalUpdates?.Files.Count ?? 0, totalFiles, errorCallback);
					}

					if (platformUpdates.RemovedFiles != null) {
						CheckRemovedFiles(platformUpdates);
					}
				}

				PopupSystem.HideMessage(progressPopup);

				if (FilesToUpdate.Count > 0) {
					Log.Info($"Update available: {CachedUpdates.Version} (Build {CachedUpdates.Build}), files to update: {FilesToUpdate.Count}; files to remove: {FilesToRemove.Count}");
					updateAvailableCallback?.Invoke(CachedUpdates);
				} else {
					Log.Info("No update available.");

					if (FilesToRemove.Count > 0) {
						DeleteRemovedFiles();
					}

					updateAvailableCallback?.Invoke(null);
				}
			})).Start();

			yield break;
		}

		private void CheckUpdatedFiles(UpdatePlatformDefinition updateDef, int startIndex, int totalFiles, Action<string> errorCallback) {
			if (updateDef.Files.Count == 0) {
				Log.Info($"Update catalog doesn't contain any files for platform \"{updateDef.Platform}\", aborting...");
				return;
			}

			foreach (var file in updateDef.Files) {
				progressPopup.Text = $"Checking files for updates...\n{file.FileName}";
				progressPopup.Progress = 0.6f + (((float)startIndex + (float)updateDef.Files.IndexOf(file)) / (float)totalFiles) * 0.4f;

				string path = $"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}\\{file.FileName}";

				if (!File.Exists(path)) {
					if (!file.IsOptional) {
						Log.Info($"Local file {file.FileName} doesn't exist");

						FilesToUpdate.Add(file);
					}

					continue;
				} else {
					try {
						var fileNeedsUpdate = false;

						if (!string.IsNullOrEmpty(file.MD5)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var md5 = MD5.Create()) {
									var md5sum = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!md5sum.Equals(file.MD5)) {
										Log.Info($"Local file {file.FileName}: MD5 doesn't match (expected {file.MD5}, got {md5sum})");
										fileNeedsUpdate = true;
									}
								}
							}
						}

						if (!string.IsNullOrEmpty(file.SHA256)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var sha256 = SHA256.Create()) {
									var sha256sum = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!sha256sum.Equals(file.SHA256)) {
										Log.Info($"Local file {file.FileName}: SHA256 doesn't match (expected {file.SHA256}, got {sha256sum})");
										fileNeedsUpdate = true;
									}
								}
							}
						}

						if (!string.IsNullOrEmpty(file.SHA512)) {
							using (FileStream fileStream = File.OpenRead(path)) {
								using (var sha512 = SHA512.Create()) {
									var sha512sum = BitConverter.ToString(sha512.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

									if (!sha512sum.Equals(file.SHA512)) {
										Log.Info($"Local file {file.FileName}: SHA512 doesn't match (expected {file.SHA512}, got {sha512sum})");
										fileNeedsUpdate = true;
									}
								}
							}
						}

						if (fileNeedsUpdate) {
							FilesToUpdate.Add(file);
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
			foreach (var file in updateDef.RemovedFiles) {
				string path = $"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}\\{file.FileName}";

				if (File.Exists(path)) {
					Log.Info($"Local file {file.FileName} exists, marked for removal");
					FilesToRemove.Add(file);
				}
			}
		}

		private void DeleteRemovedFiles() {
			if (FilesToRemove.Count == 0) return;

			var backupPath = $"{Application.dataPath}\\Updates\\bak";

			if (!Directory.Exists(backupPath)) {
				Directory.CreateDirectory(backupPath);
			}

			foreach (var file in FilesToRemove) {
				try {
					if (!Directory.Exists($"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}")) {
						continue;
					}

					var filename = $"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}\\{file.FileName}";

					if (File.Exists(filename)) {
						// File will be removed on game restart
						var fileBackupPath = Path.Combine(backupPath, Path.GetFileName(filename) + ".bak");

						if (File.Exists(fileBackupPath)) {
							File.Delete(fileBackupPath);
						}

						File.Move(filename, fileBackupPath);
					}
				} catch (Exception e) {
					Log.Error(e);
				}
			}
		}

		private IEnumerator InstallUpdates(Action updateCompleteCallback, Action<string> errorCallback) {
			if (FilesToUpdate.Count == 0) yield break;

			var backupPath = $"{Application.dataPath}\\Updates\\bak";

			if (!Directory.Exists(backupPath)) {
				Directory.CreateDirectory(backupPath);
			}

			foreach (var file in FilesToUpdate) {
				progressPopup = PopupSystem.ShowProgress($"Downloading updates... ({FilesToUpdate.IndexOf(file) + 1}/{FilesToUpdate.Count})", $"{file.FileName} ({ParadiseGUITools.FormatSize(file.FileSize)})");

				using (WWW loader = new WWW(string.Join("/", new string[] { file.RemoteURL ?? UpdateCatalogUrl, file.RemotePath, file.FileName }))) {
					Log.Info($"Downloading remote file: {loader.url}");

					while (!loader.isDone) {
						progressPopup.Progress = loader.progress;
						yield return null;
					}

					PopupSystem.HideMessage(progressPopup);

					var responseHeader = HTTPStatusParser.ParseHeader(loader.responseHeaders["STATUS"]);

					if (responseHeader.StatusCode != HttpStatusCode.OK) {
						errorCallback?.Invoke($"Failed to download {loader.url} {file.FileName}:\n{responseHeader.StatusText}");
						yield break;
					}

					try {
						if (!Directory.Exists($"{Application.dataPath}\\Updates")) {
							Directory.CreateDirectory($"{Application.dataPath}\\Updates");
						}

						if (!string.IsNullOrEmpty(file.MD5)) {
							using (var md5 = MD5.Create()) {
								var md5sum = BitConverter.ToString(md5.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!md5sum.Equals(file.MD5)) {
									errorCallback?.Invoke($"{file.FileName}: Hash sum mismatch. Please contact your server administrator.");
									Log.Error($"Downloaded file {file.FileName}: MD5 doesn't match (expected {file.MD5}, got {md5sum})");
									continue;
								}
							}
						}

						if (!string.IsNullOrEmpty(file.SHA256)) {
							using (var sha256 = SHA256.Create()) {
								var sha256sum = BitConverter.ToString(sha256.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!sha256sum.Equals(file.SHA256)) {
									errorCallback?.Invoke($"{file.FileName}: Hash sum mismatch. Please contact your server administrator.");
									Log.Error($"Downloaded file {file.FileName}: SHA256 doesn't match (expected {file.SHA256}, got {sha256sum})");
									continue;
								}
							}
						}

						if (!string.IsNullOrEmpty(file.SHA512)) {
							using (var sha512 = SHA512.Create()) {
								var sha512sum = BitConverter.ToString(sha512.ComputeHash(loader.bytes)).Replace("-", "").ToLowerInvariant();

								if (!sha512sum.Equals(file.SHA512)) {
									errorCallback?.Invoke($"{file.FileName}: Hash sum mismatch. Please contact your server administrator.");
									Log.Error($"Downloaded file {file.FileName}: SHA512 doesn't match (expected {file.SHA512}, got {sha512sum})");
									continue;
								}
							}
						}
					} catch (Exception e) {
						PopupSystem.HideMessage(progressPopup);

						errorCallback?.Invoke($"An error occured while downloading updates: {e.Message}");
						Log.Error(e);

						yield break;
					}

					try {
						var tempFilename = $"{Application.dataPath}\\Updates\\{file.FileName}";
						File.WriteAllBytes(tempFilename, loader.bytes);

						if (!Directory.Exists($"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}")) {
							Directory.CreateDirectory($"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}");
						}

						var filename = $"{Path.GetDirectoryName(Application.dataPath)}\\{file.LocalPath}\\{file.FileName}";

						if (File.Exists(filename)) {
							// File will be removed on game restart
							var fileBackupPath = Path.Combine(backupPath, Path.GetFileName(filename) + ".bak");

							if (File.Exists(fileBackupPath)) {
								File.Delete(fileBackupPath);
							}

							File.Move(filename, fileBackupPath);
						}

						File.Move(tempFilename, filename);
					} catch (Exception e) {
						PopupSystem.HideMessage(progressPopup);

						errorCallback?.Invoke($"An error occured while installing updates: {e.Message}");
						Log.Error(e);

						yield break;
					}
				}
			}

			DeleteRemovedFiles();

			updateCompleteCallback?.Invoke();
		}
	}
}
