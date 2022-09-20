using Paradise.Core.Models.Views;
using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class AuthenticateApplicationView {
		public List<PhotonView> GameServers { get; set; }

		public PhotonView CommServer { get; set; }

		public bool WarnPlayer { get; set; }

		public bool IsEnabled { get; set; }

		public string EncryptionInitVector { get; set; }

		public string EncryptionPassPhrase { get; set; }
	}
}
