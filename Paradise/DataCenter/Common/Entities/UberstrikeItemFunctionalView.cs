using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemFunctionalView : UberstrikeItemView {
		public UberstrikeItemFunctionalView() {
		}

		public UberstrikeItemFunctionalView(ItemView item, int levelRequired, UberstrikeFunctionalConfigView config) : base(item, levelRequired) {
			this.Config = config;
		}

		public UberstrikeFunctionalConfigView Config { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeFunctionalView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append(this.Config);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
