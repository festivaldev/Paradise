using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ItemQuickUseConfigViewProxy {
		public static void Serialize(Stream stream, ItemQuickUseConfigView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<QuickItemLogic>.Serialize(memoryStream, instance.BehaviourType);
				Int32Proxy.Serialize(memoryStream, instance.CoolDownTime);
				Int32Proxy.Serialize(memoryStream, instance.ItemId);
				Int32Proxy.Serialize(memoryStream, instance.LevelRequired);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerGame);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerLife);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerRound);
				Int32Proxy.Serialize(memoryStream, instance.WarmUpTime);
				memoryStream.WriteTo(stream);
			}
		}

		public static ItemQuickUseConfigView Deserialize(Stream bytes) {
			return new ItemQuickUseConfigView {
				BehaviourType = EnumProxy<QuickItemLogic>.Deserialize(bytes),
				CoolDownTime = Int32Proxy.Deserialize(bytes),
				ItemId = Int32Proxy.Deserialize(bytes),
				LevelRequired = Int32Proxy.Deserialize(bytes),
				UsesPerGame = Int32Proxy.Deserialize(bytes),
				UsesPerLife = Int32Proxy.Deserialize(bytes),
				UsesPerRound = Int32Proxy.Deserialize(bytes),
				WarmUpTime = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
