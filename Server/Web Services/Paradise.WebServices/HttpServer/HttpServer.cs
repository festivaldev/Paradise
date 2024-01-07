using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;

namespace Paradise.WebServices {
	public class HttpServer {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(HttpServer));

		protected List<HttpRouter> m_routers = new List<HttpRouter>();
		protected IDictionary<string, IDictionary<string, HttpRouter.RouteInfo>> m_routerPaths;
		protected HttpListener m_listener;
		protected IEnumerable<string> m_prefixes;

		public static HttpServer Instance;

		protected Thread _serverThread;
		public bool IsRunning => _serverThread != null && _serverThread.IsAlive;

		private static string RemoveTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri.Substring(0, uri.Length - 1) : uri;
		}

		public HttpServer(IEnumerable<string> prefixes) {
			Instance = this;

			m_prefixes = prefixes;
		}

		public void Use(HttpRouter router) {
			if (router == null) return;

			m_routers.Add(router);
		}

		private IDictionary<string, IDictionary<string, HttpRouter.RouteInfo>> CollectRoutes() {
			var routes = new Dictionary<string, IDictionary<string, HttpRouter.RouteInfo>>();

			foreach (var router in m_routers) {
				foreach (MethodInfo method in router.GetType().GetMethods()) {
					var attributes = method.GetCustomAttributes(typeof(HttpRouter.RouteAttribute), true);
					if (attributes.Length == 0) continue;

					var routingAttribute = attributes[0] as HttpRouter.RouteAttribute;
					if (!routes.ContainsKey(routingAttribute.Method)) {
						routes[routingAttribute.Method] = new Dictionary<string, HttpRouter.RouteInfo>();
					}

					var route = RemoveTrailingSlash(routingAttribute.Path);
					if (!routes[routingAttribute.Method].ContainsKey(route)) {
						routes[routingAttribute.Method][route] = new HttpRouter.RouteInfo {
							Router = router,
							Handle = method,
							Path = routingAttribute.Path,
							IsStatic = routingAttribute.Static
						};
					}
				}
			}

			return routes;
		}

		protected virtual HttpRouter.RouteInfo FindRouteHandler(string routeName, ref HttpListenerRequest request) {
			if (m_routerPaths.ContainsKey(request.HttpMethod)) {
				var route = RemoveTrailingSlash(routeName);

				foreach (var entry in m_routerPaths[request.HttpMethod]) {
					if (entry.Key.Equals(route) || (entry.Value.IsStatic && route.StartsWith(entry.Key))) {
						return entry.Value;
					}
				}
			}

			return default;
		}

		protected virtual void HandleOptionsRequest(ref HttpListenerResponse response) {
			response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, PUT, DELETE, OPTIONS");
			response.Headers.Add("Access-Control-Request-Headers", "X-PINGOTHER, Content-Type");
		}

		protected virtual void ParseRequest(HttpListenerRequest request, out string route, out NameValueCollection query, out string body) {
			route = request.Url.LocalPath;
			query = HttpUtility.ParseQueryString(request.Url.Query);

			body = "";
			if (request.HasEntityBody) {
				using (var oReader = new StreamReader(request.InputStream, request.ContentEncoding)) {
					body = oReader.ReadToEnd();
				}
			}
		}

		private void RespondWithError(HttpListenerResponse response, int statusCode, string message) {
			var bytes = Encoding.UTF8.GetBytes(message);

			response.StatusCode = statusCode;
			response.ContentEncoding = Encoding.UTF8;
			response.ContentType = "text/plain; charset=utf-8;";
			response.ContentLength64 = bytes.Length;
			response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		protected virtual void RunHandler(HttpRouter.RouteInfo handler, ref HttpListenerContext context, string route, NameValueCollection query, string body) {
			var methodParameterInfo = handler.Handle.GetParameters();
			var methodParameters = new object[methodParameterInfo.Length];

			for (int i = 0; i != methodParameterInfo.Length; i++) {
				var _parameter = methodParameterInfo[i];

				if (_parameter.Name.Equals("query") && _parameter.ParameterType == typeof(NameValueCollection)) {
					methodParameters[i] = query;
				} else if (_parameter.Name.Equals("body")) {
					if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json")) {
						try {
							methodParameters[i] = JsonConvert.DeserializeObject(body, methodParameterInfo[i].ParameterType);
						} catch (Exception e) {
							RespondWithError(context.Response, (int)HttpStatusCode.InternalServerError, $"Internal Server Error\n{e}");
							return;
						}
					} else {
						methodParameters[i] = body;
					}
				}
			}

			if (methodParameters != null) {
				try {
					Thread.SetData(Thread.GetNamedDataSlot("Request"), context.Request);
					Thread.SetData(Thread.GetNamedDataSlot("Response"), context.Response);
					Thread.SetData(Thread.GetNamedDataSlot("RouteInfo"), handler);

					handler.Handle.Invoke(handler.Router, methodParameters);
				} catch (Exception e) {
					RespondWithError(context.Response, (int)HttpStatusCode.InternalServerError, $"Internal Server Error\n{e}");
				}
			}
		}

		protected virtual bool ShouldAllowCORS() {
			return true;
		}

		public void Start() {
			Log.Info($"Starting HTTP server...");

			_serverThread = new Thread(this.Listen);
			_serverThread.Start();
		}

		private void Listen() {
			m_listener = new HttpListener {
				IgnoreWriteExceptions = true
			};

			foreach (var prefix in m_prefixes) {
				m_listener.Prefixes.Add(prefix);
			}

			m_routerPaths = CollectRoutes();

			try {
				m_listener.Start();
			} catch (HttpListenerException e) {
				Log.Error($"Failed to start HTTP server: {e.Message}");
				Log.Debug(e);

				return;
			} catch (Exception e) {
				Log.Error(e);
			}

			while (true) {
				HttpListenerContext context;

				try {
					context = m_listener.GetContext();
				} catch (Exception) {
					break;
				}

				if (context == null) continue;

				ThreadPool.QueueUserWorkItem((_) => {
					var request = context.Request;
					var response = context.Response;

					try {
						if (ShouldAllowCORS()) {
							response.Headers.Add("Access-Control-Allow-Origin", "*");
						}

						if (request.HttpMethod.Equals("OPTIONS")) {
							HandleOptionsRequest(ref response);
							return;
						}

						ParseRequest(request, out var route, out var query, out var body);

						var routeHandler = FindRouteHandler(route, ref request);
						if (routeHandler.Handle == null) {
							RespondWithError(response, (int)HttpStatusCode.NotFound, $"Cannot {request.HttpMethod} {route}");
							return;
						}

						RunHandler(routeHandler, ref context, route, query, body);
					} catch (Exception) {

					} finally {
						response.Close();
					}
				});
			}
		}

		public void Stop() {
			if (m_listener == null) return;

			if (IsRunning) {
				_serverThread.Abort();
			}

			if (m_listener.IsListening) {
				m_listener.Stop();
				m_listener.Close();
			}

			m_listener = null;

			Log.Info($"HTTP server stopped.");
		}
	}
}
