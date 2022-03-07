using Paradise.Core.Types;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemQuickUseView : UberstrikeItemView {
		public UberstrikeItemQuickUseView() {
		}

		public UberstrikeItemQuickUseView(ItemView item, int levelRequired) : base(item, levelRequired) {
		}

		public UberstrikeItemQuickUseView(ItemView item, int levelRequired, ItemQuickUseConfigView Config) : base(item, levelRequired) {
			this.Config = Config;
		}

		public ItemQuickUseConfigView Config { get; set; }

		public QuickItemLogic Logic { get; set; }
	}
}
