using System.IO;
using System;
using Paradise.Core.Serialization;

namespace Paradise.WebServices {
	public class SteamMember {
		public string SteamId { get; set; }
		public int Cmid { get; set; }
		public string AuthToken { get; set; }
		public string MachineId { get; set; }

		public static SteamMember FromAuthToken(string authToken) {
			using (var inputStream = new MemoryStream(Convert.FromBase64String(authToken))) {
				var steamId = StringProxy.Deserialize(inputStream);
				var validThru = DateTimeProxy.Deserialize(inputStream);

				if (validThru >= DateTime.UtcNow) {
					var steamMember = DatabaseManager.SteamMembers.FindOne(_ => _.SteamId == steamId);

					return steamMember;
				}
			}

			return null;
		}
	}
}
