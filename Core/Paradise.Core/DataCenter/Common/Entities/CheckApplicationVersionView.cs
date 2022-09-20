using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class CheckApplicationVersionView {
		public CheckApplicationVersionView() {
		}

		public CheckApplicationVersionView(ApplicationView clienVersion, ApplicationView currentVersion) {
			this.ClientVersion = clienVersion;
			this.CurrentVersion = currentVersion;
		}

		public ApplicationView ClientVersion { get; set; }

		public ApplicationView CurrentVersion { get; set; }

		public override string ToString() {
			return string.Concat(new object[]
			{
				"[CheckApplicationVersionView: [ClientVersion: ",
				this.ClientVersion,
				"][CurrentVersion: ",
				this.CurrentVersion,
				"]]"
			});
		}
	}
}
