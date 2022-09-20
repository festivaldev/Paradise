using System.Collections.Generic;
using System.Linq;

namespace Paradise.DataCenter.Common.Entities {
	public class FacebookBasicStatisticView : EsnsBasicStatisticView {
		public FacebookBasicStatisticView(long facebookId, string firstName, string picturePath, string name, int xp, int level, int cmid) : base(name, xp, level, cmid) {
			this.FacebookId = facebookId;
			this.FirstName = firstName;
			this.PicturePath = picturePath;
		}

		public FacebookBasicStatisticView(long facebookId, string firstName, string picturePath) {
			this.FacebookId = facebookId;
			this.FirstName = firstName;
			this.PicturePath = picturePath;
		}

		public FacebookBasicStatisticView() {
			this.FacebookId = 0L;
		}

		public long FacebookId { get; set; }

		public string FirstName { get; set; }

		public string PicturePath {
			get {
				return this._picturePath;
			}
			set {
				if (value.StartsWith("http:")) {
					value = value.Replace("http:", "https:");
				}
				this._picturePath = value;
			}
		}

		public static List<FacebookBasicStatisticView> Rank(List<FacebookBasicStatisticView> views, int friendsDisplayedCount) {
			List<FacebookBasicStatisticView> list = new List<FacebookBasicStatisticView>();
			FacebookBasicStatisticView facebookBasicStatisticView = null;
			if (views.Count > 0) {
				facebookBasicStatisticView = views[0];
			}
			views = (from v in views
					 orderby v.XP descending
					 select v).ToList<FacebookBasicStatisticView>();
			int num = 1;
			foreach (FacebookBasicStatisticView facebookBasicStatisticView2 in views) {
				if (facebookBasicStatisticView2.Cmid != 0) {
					facebookBasicStatisticView2.SocialRank = num;
					num++;
				}
			}
			list.Add(facebookBasicStatisticView);
			num = 0;
			int num2 = 0;
			while (num2 < friendsDisplayedCount && num2 < views.Count) {
				if (views[num2].FacebookId != facebookBasicStatisticView.FacebookId) {
					list.Add(views[num2]);
					num++;
				}
				num2++;
			}
			while (list.Count < friendsDisplayedCount + 1) {
				list.Add(new FacebookBasicStatisticView());
			}
			return list;
		}

		private string _picturePath;
	}
}
