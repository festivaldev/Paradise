using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Paradise.Client.DiscordRPC {
	internal class Program {
		static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

		static readonly Mutex mutex = new Mutex(true, "{7b37f0ce-4a3e-4735-a713-2bf27277ad74}");
		static readonly string PresenceFile = Path.Combine(Path.GetTempPath(), "7b37f0ce-4a3e-4735-a713-2bf27277ad74");

		private static object Lock = new object();


		[STAThread]
		static void Main(string[] args) {
			if (!mutex.WaitOne(TimeSpan.Zero, true)) {
				return;
			}

			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
				var assemblyName = new AssemblyName(e.Name).Name;
				var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(_ => _.EndsWith($"{assemblyName}.dll"));

				if (string.IsNullOrEmpty(resourceName)) return null;

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
					return Assembly.Load(new BinaryReader(stream).ReadBytes((int)stream.Length));
				}
			};

			try {
				RichPresenceManager.Initialize();

				var watcher = new FileSystemWatcher(Path.GetDirectoryName(PresenceFile), Path.GetFileName(PresenceFile));
				watcher.NotifyFilter = NotifyFilters.LastWrite;
				watcher.Changed += (object sender, FileSystemEventArgs eArgs) => {
					try {
						watcher.EnableRaisingEvents = false;

						ReadPresenceFile();
					} catch (Exception e) {
						Console.WriteLine(e);
					} finally {
						watcher.EnableRaisingEvents = true;
					}
				};

				try {
					ReadPresenceFile();
				} catch (Exception e) {
					Console.WriteLine(e);
				}

				watcher.EnableRaisingEvents = true;
			} catch (Exception e) {
				Console.WriteLine(e);
			}

			QuitEvent.WaitOne();
		}

		private static void ReadPresenceFile() {
			lock (Lock) {
				using (var stream = File.Open(PresenceFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) {
					using (var reader = new StreamReader(stream)) {
						var content = Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));

						if (!string.IsNullOrWhiteSpace(content)) {
							var serializer = new XmlSerializer(typeof(RichPresenceSerializable));
							using (var xmlReader = new StringReader(content)) {
								RichPresenceManager.SetPresence((RichPresenceSerializable)serializer.Deserialize(xmlReader));
							}
						}

						stream.SetLength(0);
						stream.Close();
					}
				}
			}
		}
	}
}
