using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemWeaponView : UberstrikeItemView {
		public UberstrikeItemWeaponView() {
		}

		public UberstrikeItemWeaponView(ItemView item, int levelRequired, UberstrikeWeaponConfigView config) : base(item, levelRequired) {
			this.Config = config;
		}

		public UberstrikeWeaponConfigView Config { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeWeaponView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(this.Config);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
