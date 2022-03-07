using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemView : ItemView {
		public UberstrikeItemView() {
		}

		public UberstrikeItemView(ItemView item, int levelRequired) : base(item) {
			this.LevelRequired = levelRequired;
		}

		public int LevelRequired { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeItemView: ");
			stringBuilder.Append(base.ToString());
			stringBuilder.Append("[LevelRequired: ");
			stringBuilder.Append(this.LevelRequired);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
