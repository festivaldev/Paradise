using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemGearView : UberstrikeItemView {
		public UberstrikeItemGearView() {
		}

		public UberstrikeItemGearView(ItemView item, int levelRequired, UberstrikeGearConfigView config) : base(item, levelRequired) {
			this.Config = config;
		}

		public UberstrikeGearConfigView Config { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeGearView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(this.Config);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
