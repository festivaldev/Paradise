// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html

using log4net;
using Paradise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

class SimpleHTTPServer {
	protected static readonly ILog Log = LogManager.GetLogger(nameof(SimpleHTTPServer));

	public static SimpleHTTPServer Instance;
	protected ParadiseServerSettings Settings;

	private readonly string[] _indexFiles = {
		"index.html",
		"index.htm",
		"default.html",
		"default.htm"
	};

	private static readonly IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
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
	private Thread _serverThread;
	private string _rootDirectory;
	private HttpListener _listener;
	private int _port;

	public int Port {
		get { return _port; }
		private set { }
	}

	public SimpleHTTPServer(string path, ParadiseServerSettings settings) {
		Instance = this;

		Settings = settings;

		this.Initialize(path, settings.FileServerPort);
	}

	/// <summary>
	/// Starts server
	/// </summary>
	public void Start() {
		Log.Info($"Starting HTTP server...");

		_serverThread = new Thread(this.Listen);
		_serverThread.Start();
	}

	/// <summary>
	/// Stop server and dispose all functions.
	/// </summary>
	public void Stop() {
		_serverThread.Abort();
		_listener.Stop();

		Log.Info($"HTTP server stopped.");
	}

	public Thread ServerThread => _serverThread;

	public bool IsRunning => _serverThread != null && _serverThread.IsAlive;

	private void Listen() {
		_listener = new HttpListener();

		// SSL certificate is set using HttpConfig.exe
		var uriBuilder = new UriBuilder {
			Scheme = Settings.EnableSSL ? "https" : "http",
			Host = string.IsNullOrEmpty(Settings.FileServerHostName) ? "*" : Settings.FileServerHostName,
			Port = _port
		};

		_listener.Prefixes.Add(uriBuilder.ToString());

		try {
			_listener.Start();
		} catch (HttpListenerException e) {
			Log.Error($"Failed to start HTTP server: {e.Message}");
			Log.Debug(e);

			return;
		} catch (Exception e) {
			Log.Error(e);
		}

		Log.Info($"HTTP server listening on port {_port} (using SSL: {(Settings.EnableSSL ? "yes" : "no")}).");

		while (true) {
			try {
				HttpListenerContext context = _listener.GetContext();
				Process(context);
			} catch (ThreadAbortException e) {
				Log.Info("HTTP server has thrown ThreadAbortException, this is expected (I guess)");
				Log.Debug(e);
			} catch (Exception e) {
				Log.Error($"Failed to start HTTP server: {e.Message}");
				Log.Debug(e);

				break;
			}
		}
	}

	private void Process(HttpListenerContext context) {
		string filename = context.Request.Url.AbsolutePath;
		filename = filename.Substring(1);

		if (string.IsNullOrEmpty(filename)) {
			foreach (string indexFile in _indexFiles) {
				if (File.Exists(Path.Combine(_rootDirectory, indexFile))) {
					filename = indexFile;
					break;
				}
			}
		}

		filename = Path.Combine(_rootDirectory, filename);

		if (File.Exists(filename)) {
			try {
				Stream input = new FileStream(filename, FileMode.Open);
				//Adding permanent http response headers
				context.Response.StatusCode = (int)HttpStatusCode.OK;
				context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out string mime) ? mime : "application/octet-stream";
				context.Response.ContentLength64 = input.Length;
				context.Response.AddHeader("Date", DateTime.UtcNow.ToString("r"));
				context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

				byte[] buffer = new byte[1024 * 16];
				int nbytes;
				while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
					context.Response.OutputStream.Write(buffer, 0, nbytes);
				input.Close();
			} catch (Exception e) {
#if DEBUG
				var error = Encoding.UTF8.GetBytes($"500 Internal Server Error\n\n{e}");
#else
				var error = Encoding.UTF8.GetBytes($"500 Internal Server");
#endif

				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.ContentEncoding = Encoding.UTF8;
				context.Response.ContentType = "text/plain; charset=utf-8;";
				context.Response.ContentLength64 = error.Length;
				context.Response.OutputStream.Write(error, 0, error.Length);

				Log.Error(e);
			}

		} else {
			var error = Encoding.UTF8.GetBytes($"404 Not Found: {context.Request.Url.AbsolutePath}");

			context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.ContentType = "text/plain; charset=utf-8;";
			context.Response.ContentLength64 = error.Length;
			context.Response.OutputStream.Write(error, 0, error.Length);
			
		}

		context.Response.OutputStream.Flush();
		context.Response.OutputStream.Close();
	}

	private void Initialize(string path, int port) {
		this._rootDirectory = path;
		this._port = port;
	}
}