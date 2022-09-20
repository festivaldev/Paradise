using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemWeaponModView : UberstrikeItemView {
		public UberstrikeItemWeaponModView() {
		}

		public UberstrikeItemWeaponModView(ItemView item, int level, UberstrikeWeaponModConfigView config) : base(item, level) {
			this.Config = config;
		}

		public UberstrikeWeaponModConfigView Config { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeWeaponModView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(this.Config);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
