using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Paradise.WebServices {
	public class HttpRouter {
		public struct RouteInfo {
			public HttpRouter Router;
			public MethodInfo Handle;
			public string Path;
			public bool IsStatic;
		}

		public class RouteAttribute : Attribute {
			public string Method = "GET";
			public string Path = "/";
			public bool Static;

			public HttpMethod HttpMethod {
				get {
					switch (Method) {
						case "GET":
							return HttpMethod.Get;
						case "POST":
							return HttpMethod.Post;
						case "DELETE":
							return HttpMethod.Delete;
						case "PUT":
							return HttpMethod.Put;
						default: throw new ArgumentException(nameof(Method));
					}
				}
			}

			public RouteAttribute() { }
		}

		public HttpListenerRequest Request {
			get {
				return (HttpListenerRequest)Thread.GetData(Thread.GetNamedDataSlot("Request"));
			}
		}

		public HttpListenerResponse Response {
			get {
				return (HttpListenerResponse)Thread.GetData(Thread.GetNamedDataSlot("Response"));
			}
		}

		public string Path {
			get {
				return ((RouteInfo)Thread.GetData(Thread.GetNamedDataSlot("RouteInfo"))).Path;
			}
		}


		public void SetStatus(HttpStatusCode code) {
			Response.StatusCode = (int)code;
		}

		public void SetStatus(int code) {
			Response.StatusCode = code;
		}

		public CookieCollection Cookies {
			get {
				return Request.Cookies;
			}
		}

		public void SetCookie(Cookie cookie) {
			Response.Cookies.Add(cookie);
		}

		public void SetContentType(string contentType) {
			Response.ContentType = contentType;
		}

		public void Send(string content) {
			Response.ContentLength64 = content.Length;

			using (TextWriter writer = new StreamWriter(Response.OutputStream)) {
				writer.Write(content);
			}
		}

		public void Send(object content) {
			var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));

			Response.ContentEncoding = Encoding.UTF8;
			Response.ContentLength64 = bytes.LongLength;
			Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		public void Send(byte[] content) {
			Response.ContentEncoding = Encoding.UTF8;
			Response.ContentLength64 = content.LongLength;
			Response.OutputStream.Write(content, 0, content.Length);
		}
	}
}
