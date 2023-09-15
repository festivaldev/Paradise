using log4net;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.WebServices.Services {
	internal class GameSessionManager {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(GameSessionManager));

		private long Seed = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();

		public static GameSessionManager Instance { get; private set; }
		private static readonly System.Timers.Timer GarbageCollector;

		static GameSessionManager() {
			Instance = new GameSessionManager();

			GarbageCollector = new System.Timers.Timer {
				Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
			};
			GarbageCollector.Elapsed += (sender, e) => {
				DatabaseClient.GameSessions.DeleteMany(_ => _.ExpireTime <= DateTime.UtcNow);
			};
			GarbageCollector.Start();
		}

		public GameSession FindOrCreateSession(PublicProfileView profile, string machineId, SteamMember steamMember) {
			if (TryGetValue(profile.Cmid, out var session)) {
				return session;
			}

			session = new GameSession {
				SessionId = CreateSessionId(profile.Cmid, Int64.Parse(steamMember.SteamId)),
				Cmid = profile.Cmid,
				MachineId = machineId
			};

			DatabaseClient.GameSessions?.Insert(session);

			return session;
		}

		public GameSession RemoveSession(int id) {
			var session = DatabaseClient.GameSessions.FindOne(_ => _.Cmid == id);

			if (session != null) {
				DatabaseClient.GameSessions.DeleteMany(_ => _.Cmid == id);
			}

			return session;
		}

		public GameSession RemoveSession(string sessionId) {
			return RemoveSession(GameSession.GetCmidFromSessionId(sessionId));
		}

		public bool TryGetValue(int id, out GameSession session) {
			session = DatabaseClient.GameSessions.FindOne(_ => _.Cmid == id && _.ExpireTime > DateTime.UtcNow);

			if (session != null) {
				session.ExtendExpireTime();

				return true;
			}

			return false;
		}

		public bool TryGetValue(string sessionId, out GameSession session) {
			return TryGetValue(GameSession.GetCmidFromSessionId(sessionId), out session);
		}

		private string CreateSessionId(int cmid, long steamId) {
			byte[] sessionId = new byte[20];
			long seed = Seed;
			Seed = (Seed + 1) & 0xFFFFFFFFFFFFFF;

			sessionId[0] = (byte)(cmid >> 24);
			sessionId[1] = (byte)(cmid >> 16);
			sessionId[2] = (byte)(cmid >> 8);
			sessionId[3] = (byte)cmid;
			sessionId[4] = (byte)(steamId >> 56);
			sessionId[5] = (byte)(steamId >> 48);
			sessionId[6] = (byte)(steamId >> 40);
			sessionId[7] = (byte)(steamId >> 32);
			sessionId[8] = (byte)(steamId >> 24);
			sessionId[9] = (byte)(steamId >> 16);
			sessionId[10] = (byte)(steamId >> 8);
			sessionId[11] = (byte)steamId;
			sessionId[12] = (byte)(seed >> 56);
			sessionId[13] = (byte)(seed >> 48);
			sessionId[14] = (byte)(seed >> 40);
			sessionId[15] = (byte)(seed >> 32);
			sessionId[16] = (byte)(seed >> 24);
			sessionId[17] = (byte)(seed >> 16);
			sessionId[18] = (byte)(seed >> 8);
			sessionId[19] = (byte)seed;

			return Convert.ToBase64String(sessionId);
		}
	}
}
