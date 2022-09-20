using System;

namespace Paradise.Core.Models {
	[Serializable]
	public class PhotonServerLoad {
		public int PeersConnected { get; set; }
		public int PlayersConnected { get; set; }
		public int RoomsCreated { get; set; }
		public float MaxPlayerCount { get; set; }

		public int Latency;
		public DateTime TimeStamp;
		public PhotonServerLoad.Status State;

		public enum Status {
			None,
			Alive,
			NotReachable
		}
	}
}
