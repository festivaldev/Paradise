using LiteDB;

namespace Paradise.WebServices.Services {
	public class SteamMember {
		[BsonId]
		public string SteamId { get; set; }
		public int Cmid { get; set; }
		public string AuthToken { get; set; }
		public string MachineId { get; set; }
	}
}
