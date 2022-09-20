using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Paradise.Client {
	internal struct HttpResponse {
		public string ProtocolVersion;
		public HttpStatusCode StatusCode;
		public string StatusDescription;
		public string StatusText;
	}
	internal class HTTPStatusParser {
		public static HttpResponse ParseHeader(string statusHeader) {
			var headerItems = statusHeader.Split(' ');

			return new HttpResponse() {
				ProtocolVersion = headerItems[0],
				StatusCode = (HttpStatusCode)int.Parse(headerItems[1]),
				StatusDescription = string.Join(" ", headerItems.Skip(2).Take(headerItems.Count() - 2).ToArray()),
				StatusText = statusHeader
			};
		}
	}
}
