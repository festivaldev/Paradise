using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeGearConfigView {
		public int ArmorPoints { get; set; }

		public int ArmorWeight { get; set; }

		public int LevelRequired { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeGearConfigView: [ArmorPoints: ");
			stringBuilder.Append(this.ArmorPoints);
			stringBuilder.Append("][ArmorWeight: ");
			stringBuilder.Append(this.ArmorWeight);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
