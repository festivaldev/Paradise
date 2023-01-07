using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Timers;

namespace Paradise.WebServices {
	internal class MonitoringProxy : ClientBase<IParadiseMonitoring> {
		public MonitoringProxy(string target)
			: base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IParadiseMonitoring)),
				new NetNamedPipeBinding(), new EndpointAddress($"net.pipe://localhost/NewParadise.Monitoring.{target}"))) {

		}

		public string GetStatus() {
			return Channel.GetStatus();
		}

		public byte Ping() {
			return Channel.Ping();
		}
	}

	internal class ParadiseRouter : Router {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseService));

		private string WebRoot;
		private ParadiseServerSettings Settings;

		private MonitoringProxy CommMonitoringProxy = new MonitoringProxy("Comm");
		private MonitoringProxy GameMonitoringProxy = new MonitoringProxy("Game");

		private System.Timers.Timer CommPingTimer;
		private System.Timers.Timer GamePingTimer;

		private static readonly IDictionary<string, string> mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
			#region extension to MIME type list
			{".asf", "video/x-ms-asf"},
			{".asx", "video/x-ms-asf"},
			{".avi", "video/x-msvideo"},
			{".bin", "application/octet-stream"},
			{".cco", "application/x-cocoa"},
			{".crt", "application/x-x509-ca-cert"},
			{".css", "text/css"},
			{".deb", "application/octet-stream"},
			{".der", "application/x-x509-ca-cert"},
			{".dll", "application/octet-stream"},
			{".dmg", "application/octet-stream"},
			{".ear", "application/java-archive"},
			{".eot", "application/octet-stream"},
			{".exe", "application/octet-stream"},
			{".flv", "video/x-flv"},
			{".gif", "image/gif"},
			{".hqx", "application/mac-binhex40"},
			{".htc", "text/x-component"},
			{".htm", "text/html"},
			{".html", "text/html"},
			{".ico", "image/x-icon"},
			{".img", "application/octet-stream"},
			{".iso", "application/octet-stream"},
			{".jar", "application/java-archive"},
			{".jardiff", "application/x-java-archive-diff"},
			{".jng", "image/x-jng"},
			{".jnlp", "application/x-java-jnlp-file"},
			{".jpeg", "image/jpeg"},
			{".jpg", "image/jpeg"},
			{".js", "application/x-javascript"},
			{".mml", "text/mathml"},
			{".mng", "video/x-mng"},
			{".mov", "video/quicktime"},
			{".mp3", "audio/mpeg"},
			{".mpeg", "video/mpeg"},
			{".mpg", "video/mpeg"},
			{".msi", "application/octet-stream"},
			{".msm", "application/octet-stream"},
			{".msp", "application/octet-stream"},
			{".pdb", "application/x-pilot"},
			{".pdf", "application/pdf"},
			{".pem", "application/x-x509-ca-cert"},
			{".pl", "application/x-perl"},
			{".pm", "application/x-perl"},
			{".png", "image/png"},
			{".prc", "application/x-pilot"},
			{".ra", "audio/x-realaudio"},
			{".rar", "application/x-rar-compressed"},
			{".rpm", "application/x-redhat-package-manager"},
			{".rss", "text/xml"},
			{".run", "application/x-makeself"},
			{".sea", "application/x-sea"},
			{".shtml", "text/html"},
			{".sit", "application/x-stuffit"},
			{".swf", "application/x-shockwave-flash"},
			{".tcl", "application/x-tcl"},
			{".tk", "application/x-tcl"},
			{".txt", "text/plain"},
			{".war", "application/java-archive"},
			{".wbmp", "image/vnd.wap.wbmp"},
			{".wmv", "video/x-ms-wmv"},
			{".xml", "text/xml"},
			{".xpi", "application/x-xpinstall"},
			{".zip", "application/zip"},
			#endregion
		};

		public ParadiseRouter(string webRoot, ParadiseServerSettings settings) {
			WebRoot = System.IO.Path.GetFullPath(webRoot);
			Settings = settings;

			CommPingTimer = new System.Timers.Timer(2000);
			CommPingTimer.Elapsed += ((object source, ElapsedEventArgs e) => {
				try {
					if (CommMonitoringProxy.State == CommunicationState.Faulted) {
						CommMonitoringProxy.Abort();
						CommMonitoringProxy = new MonitoringProxy("Comm");
					}

					CommMonitoringProxy.Ping();
				} catch (Exception ex) {
					
				}

			});
			CommPingTimer.Start();

			GamePingTimer = new System.Timers.Timer(2000);
			GamePingTimer.Elapsed += ((object source, ElapsedEventArgs e) => {
				try {
					if (GameMonitoringProxy.State == CommunicationState.Faulted) {
						GameMonitoringProxy.Abort();
						GameMonitoringProxy = new MonitoringProxy("Game");
					}

					GameMonitoringProxy.Ping();
				} catch (Exception ex) {
					
				}

			});
			GamePingTimer.Start();
		}

		[Route(Path = "/status/web")]
		public void WebServicesStatus() {
			Console.WriteLine("called test route");
		}

		[Route(Path = "/status/comm")]
		public bool CommServerStatus() {
			string status = null;

			try {
				status = CommMonitoringProxy.GetStatus();
			} catch (Exception e) {
				if (e is CommunicationException || e is EndpointNotFoundException) {
					SetStatus(503);
					Send($"503 Service Temporarily Unavailable");
					Log.Error(e);
				} else {
					SetStatus(500);
					Send($"500 Internal Server Error");
					Log.Error(e);
				}

				CommMonitoringProxy.Abort();
				CommMonitoringProxy = new MonitoringProxy("Comm");

				return true;
			}

			SetStatus(200);
			SetContentType("application/json");
			Send(JsonConvert.DeserializeObject(status));

			return true;
		}

		[Route(Path = "/status/game")]
		public bool GameServerStatus() {
			string status = null;

			try {
				status = GameMonitoringProxy.GetStatus();
			} catch (Exception e) {
				if (e is CommunicationException || e is EndpointNotFoundException) {
					SetStatus(503);
					Send($"503 Service Temporarily Unavailable");
					Log.Error(e);
				} else {
					SetStatus(500);
					Send($"500 Internal Server Error");
					Log.Error(e);
				}

				GameMonitoringProxy.Abort();
				GameMonitoringProxy = new MonitoringProxy("Game");

				return true;
			}

			SetStatus(200);
			SetContentType("application/json");
			Send(JsonConvert.DeserializeObject(status));

			return true;
		}

		[Route(Path = "/", Static = true)]
		public void StaticFiles() {
			string realPath = Request.Url.AbsolutePath;

			if (realPath.StartsWith("/")) {
				realPath = realPath.Substring(1, realPath.Length - 1);
			}

			var filename = System.IO.Path.Combine(WebRoot, realPath);

			if (!(new Uri(WebRoot).IsBaseOf(new Uri(filename)))) {
				SetStatus(403);
				Send("403 Forbidden");
				return;
			}

			if (File.Exists(filename)) {
				SetContentType(mimeTypes.TryGetValue(System.IO.Path.GetExtension(filename), out string mime) ? mime : "application/octet-stream");
				Send(File.ReadAllBytes(filename));
			} else {
				SetStatus(404);
				Send($"404 Not Found");
			}
		}
	}
}
