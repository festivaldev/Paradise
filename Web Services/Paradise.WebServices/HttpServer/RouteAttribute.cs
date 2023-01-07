using System;
using System.Net.Http;

namespace Paradise.WebServices {
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
}
