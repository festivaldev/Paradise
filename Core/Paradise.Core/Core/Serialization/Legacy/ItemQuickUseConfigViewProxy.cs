using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ItemQuickUseConfigViewProxy {
		public static void Serialize(Stream stream, ItemQuickUseConfigView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					EnumProxy<QuickItemLogic>.Serialize(memoryStream, instance.BehaviourType);
					Int32Proxy.Serialize(memoryStream, instance.CoolDownTime);
					Int32Proxy.Serialize(memoryStream, instance.ItemId);
					Int32Proxy.Serialize(memoryStream, instance.LevelRequired);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerGame);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerLife);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerRound);
					Int32Proxy.Serialize(memoryStream, instance.WarmUpTime);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ItemQuickUseConfigView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ItemQuickUseConfigView itemQuickUseConfigView = null;
			if (num != 0) {
				itemQuickUseConfigView = new ItemQuickUseConfigView();
				itemQuickUseConfigView.BehaviourType = EnumProxy<QuickItemLogic>.Deserialize(bytes);
				itemQuickUseConfigView.CoolDownTime = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.ItemId = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.LevelRequired = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.UsesPerGame = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.UsesPerLife = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.UsesPerRound = Int32Proxy.Deserialize(bytes);
				itemQuickUseConfigView.WarmUpTime = Int32Proxy.Deserialize(bytes);
			}
			return itemQuickUseConfigView;
		}
	}
}
