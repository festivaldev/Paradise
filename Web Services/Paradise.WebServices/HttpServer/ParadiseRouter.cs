using log4net;
using Paradise.WebServices.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.WebServices {

	internal class ParadiseRouter : Router {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseService));

		private string WebRoot;
		private ParadiseServerSettings Settings;

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
		}

		[Route(Path = "/status/web")]
		public void WebServicesStatus() {
			Console.WriteLine("called test route");
		}

		[Route(Path = "/status/comm")]
		public bool CommServerStatus() {
			SetStatus(200);
			SetContentType("application/json");
			Send(ApplicationWebService.CommMonitoringData.Values);

			return true;
		}

		[Route(Path = "/status/game")]
		public bool GameServerStatus() {
			SetStatus(200);
			SetContentType("application/json");
			Send(ApplicationWebService.GameMonitoringData.Values);

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
