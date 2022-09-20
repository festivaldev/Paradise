using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PlayerCardView : IComparable {
		public PlayerCardView() {
			this.Name = string.Empty;
			this.Precision = string.Empty;
			this.TagName = string.Empty;
		}

		public PlayerCardView(int cmid, int splats, int splatted, long shots, long hits) {
			this.Cmid = cmid;
			this.Splats = splats;
			this.Splatted = splatted;
			this.Shots = shots;
			this.Hits = hits;
		}

		public PlayerCardView(int cmid, string name, int splats, int splatted, string precision, int ranking, string tagName) {
			this.Cmid = cmid;
			this.Name = name;
			this.Splats = splats;
			this.Splatted = splatted;
			this.Precision = precision;
			this.Ranking = ranking;
			this.TagName = tagName;
		}

		public PlayerCardView(string name, int splats, int splatted, string precision, int ranking, string tagName) {
			this.Name = name;
			this.Splats = splats;
			this.Splatted = splatted;
			this.Precision = precision;
			this.Ranking = ranking;
			this.TagName = tagName;
		}

		public PlayerCardView(int cmid, string name, int splats, int splatted, string precision, int ranking, long shots, long hits, string tagName) {
			this.Cmid = cmid;
			this.Name = name;
			this.Splats = splats;
			this.Splatted = splatted;
			this.Precision = precision;
			this.Ranking = ranking;
			this.Shots = shots;
			this.Hits = hits;
			this.TagName = tagName;
		}

		public PlayerCardView(string name, int splats, int splatted, string precision, int ranking, long shots, long hits, string tagName) {
			this.Name = name;
			this.Splats = splats;
			this.Splatted = splatted;
			this.Precision = precision;
			this.Ranking = ranking;
			this.Shots = shots;
			this.Hits = hits;
			this.TagName = tagName;
		}

		public string Name { get; set; }

		public int Cmid { get; set; }

		public int Splats { get; set; }

		public int Splatted { get; set; }

		public string Precision { get; set; }

		public int Ranking { get; set; }

		public long Shots { get; set; }

		public long Hits { get; set; }

		public string TagName { get; set; }

		public int CompareTo(object obj) {
			if (obj is PlayerCardView) {
				PlayerCardView playerCardView = obj as PlayerCardView;
				return -(playerCardView.Ranking - this.Ranking);
			}
			throw new ArgumentOutOfRangeException("Parameter is not of the good type");
		}

		public override string ToString() {
			return string.Concat(new object[]
			{
				"[Player: [Name: ",
				this.Name,
				"][Cmid: ",
				this.Cmid,
				"][Splats: ",
				this.Splats,
				"][Shots: ",
				this.Shots,
				"][Hits: ",
				this.Hits,
				"][Splatted: ",
				this.Splatted,
				"][Precision: ",
				this.Precision,
				"][Ranking: ",
				this.Ranking,
				"][TagName: ",
				this.TagName,
				"]]"
			});
		}
	}
}
