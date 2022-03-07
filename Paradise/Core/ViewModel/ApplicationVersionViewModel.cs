using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Core.ViewModel {
	public class ApplicationVersionViewModel {
		public int ApplicationVersionId { get; set; }

		public string Version { get; set; }

		public string WebPlayerFileName { get; set; }

		public ChannelType Channel { get; set; }

		public DateTime ModificationDate { get; set; }

		public bool IsEnabled { get; set; }

		public bool WarnPlayer { get; set; }

		public int PhotonClusterId { get; set; }

		public string PhotonClusterName { get; set; }

		public bool IsValid(out string invalidStates) {
			bool flag = !string.IsNullOrEmpty(this.Version) && this.Channel > (ChannelType)(-1) && this.ModificationDate > DateTime.MinValue && this.PhotonClusterId > 0;
			invalidStates = string.Empty;
			if (!flag) {
				invalidStates = "Invalid Model, unknown version";
			}
			return flag;
		}
	}
}
