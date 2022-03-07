using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemSpecialView : UberstrikeItemView {
		public UberstrikeItemSpecialView() {
		}

		public UberstrikeItemSpecialView(ItemView item, int levelRequired, UberstrikeSpecialConfigView config) : base(item, levelRequired) {
			this.Config = config;
		}

		public UberstrikeSpecialConfigView Config { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeSpecialView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(this.Config);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
