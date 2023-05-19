using LiteDB;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Linq;

namespace Paradise.WebServices.Services {
	public class GameSession {
		const int SESSION_EXPIRE_HOURS = 12;

		public string SessionId { get; set; }
		public int Cmid { get; set; }
		public string MachineId { get; set; }
		public DateTime ExpireTime { get; set; }

		[BsonIgnore]
		public PublicProfileView Profile {
			get {
				return DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == Cmid);
			}
		}

		[BsonIgnore]
		public SteamMember SteamMember {
			get {
				return DatabaseClient.SteamMembers.FindOne(_ => _.Cmid == Cmid);
			}
		}

		public GameSession() {
			ExpireTime = DateTime.UtcNow.AddHours(SESSION_EXPIRE_HOURS);
		}

		public void ExtendExpireTime(int hours = SESSION_EXPIRE_HOURS) {
			ExpireTime = DateTime.UtcNow.AddHours(hours);

			DatabaseClient.GameSessions.DeleteMany(_ => _.Cmid == Cmid);
			DatabaseClient.GameSessions.Insert(this);
		}

		public static int GetCmidFromSessionId(string sessionId) {
			if (string.IsNullOrEmpty(sessionId)) return -1;

			return BitConverter.ToInt32(Convert.FromBase64String(sessionId).Take(4).Reverse().ToArray(), 0);
		}

		public static long GetSteamIdFromSessionId(string sessionId) {
			if (string.IsNullOrEmpty(sessionId)) return -1;

			return BitConverter.ToInt64(Convert.FromBase64String(sessionId).Take(12).Reverse().ToArray(), 0);
		}
	}
}
