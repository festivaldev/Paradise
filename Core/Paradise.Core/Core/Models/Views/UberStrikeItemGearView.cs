using Paradise.Core.Types;
using System;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class UberStrikeItemGearView : BaseUberStrikeItemView {
		public override UberstrikeItemType ItemType {
			get {
				return UberstrikeItemType.Gear;
			}
		}

		public int ArmorPoints { get; set; }

		public int ArmorWeight { get; set; }

		public int ArmorAbsorptionPercent { get; set; } // # LEGACY # //
	}
}
