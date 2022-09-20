using Paradise.Core.Models;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class StatsSummaryProxy {
		public static void Serialize(Stream stream, StatsSummary instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Achievements != null) {
					DictionaryProxy<byte, ushort>.Serialize(memoryStream, instance.Achievements, new DictionaryProxy<byte, ushort>.Serializer<byte>(ByteProxy.Serialize), new DictionaryProxy<byte, ushort>.Serializer<ushort>(UInt16Proxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				Int32Proxy.Serialize(memoryStream, instance.Deaths);
				Int32Proxy.Serialize(memoryStream, instance.Kills);
				Int32Proxy.Serialize(memoryStream, instance.Level);
				if (instance.Name != null) {
					StringProxy.Serialize(memoryStream, instance.Name);
				} else {
					num |= 2;
				}
				EnumProxy<TeamID>.Serialize(memoryStream, instance.Team);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static StatsSummary Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			StatsSummary statsSummary = new StatsSummary();
			if ((num & 1) != 0) {
				statsSummary.Achievements = DictionaryProxy<byte, ushort>.Deserialize(bytes, new DictionaryProxy<byte, ushort>.Deserializer<byte>(ByteProxy.Deserialize), new DictionaryProxy<byte, ushort>.Deserializer<ushort>(UInt16Proxy.Deserialize));
			}
			statsSummary.Cmid = Int32Proxy.Deserialize(bytes);
			statsSummary.Deaths = Int32Proxy.Deserialize(bytes);
			statsSummary.Kills = Int32Proxy.Deserialize(bytes);
			statsSummary.Level = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				statsSummary.Name = StringProxy.Deserialize(bytes);
			}
			statsSummary.Team = EnumProxy<TeamID>.Deserialize(bytes);
			return statsSummary;
		}
	}
}
