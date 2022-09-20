namespace Paradise.DataCenter.Common.Entities {
	public class EsnsBasicStatisticView {
		public EsnsBasicStatisticView(string name, int xp, int level, int cmid) {
			this.Name = name;
			this.XP = xp;
			this.Level = level;
			this.Cmid = cmid;
		}

		public EsnsBasicStatisticView() {
			this.Name = string.Empty;
			this.XP = 0;
			this.Level = 0;
			this.Cmid = 0;
		}

		public string Name { get; protected set; }

		public int SocialRank { get; protected set; }

		public int XP { get; protected set; }

		public int Level { get; protected set; }

		public int Cmid { get; protected set; }
	}
}
