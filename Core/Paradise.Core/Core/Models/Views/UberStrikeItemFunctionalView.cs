using Paradise.Core.Types;
using System;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class UberStrikeItemFunctionalView : BaseUberStrikeItemView {
		public override UberstrikeItemType ItemType {
			get {
				return UberstrikeItemType.Functional;
			}
		}
	}
}
