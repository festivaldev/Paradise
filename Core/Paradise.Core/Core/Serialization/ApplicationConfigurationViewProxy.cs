using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ApplicationConfigurationViewProxy {
		public static void Serialize(Stream stream, ApplicationConfigurationView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.MaxLevel);
				Int32Proxy.Serialize(memoryStream, instance.MaxXp);
				Int32Proxy.Serialize(memoryStream, instance.PointsBaseLoser);
				Int32Proxy.Serialize(memoryStream, instance.PointsBaseWinner);
				Int32Proxy.Serialize(memoryStream, instance.PointsHeadshot);
				Int32Proxy.Serialize(memoryStream, instance.PointsKill);
				Int32Proxy.Serialize(memoryStream, instance.PointsNutshot);
				Int32Proxy.Serialize(memoryStream, instance.PointsPerMinuteLoser);
				Int32Proxy.Serialize(memoryStream, instance.PointsPerMinuteWinner);
				Int32Proxy.Serialize(memoryStream, instance.PointsSmackdown);
				Int32Proxy.Serialize(memoryStream, instance.XpBaseLoser);
				Int32Proxy.Serialize(memoryStream, instance.XpBaseWinner);
				Int32Proxy.Serialize(memoryStream, instance.XpHeadshot);
				Int32Proxy.Serialize(memoryStream, instance.XpKill);
				Int32Proxy.Serialize(memoryStream, instance.XpNutshot);
				Int32Proxy.Serialize(memoryStream, instance.XpPerMinuteLoser);
				Int32Proxy.Serialize(memoryStream, instance.XpPerMinuteWinner);
				if (instance.XpRequiredPerLevel != null) {
					DictionaryProxy<int, int>.Serialize(memoryStream, instance.XpRequiredPerLevel, new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize), new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.XpSmackdown);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ApplicationConfigurationView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ApplicationConfigurationView applicationConfigurationView = new ApplicationConfigurationView();
			applicationConfigurationView.MaxLevel = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.MaxXp = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsBaseLoser = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsBaseWinner = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsHeadshot = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsKill = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsNutshot = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsPerMinuteLoser = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsPerMinuteWinner = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.PointsSmackdown = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpBaseLoser = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpBaseWinner = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpHeadshot = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpKill = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpNutshot = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpPerMinuteLoser = Int32Proxy.Deserialize(bytes);
			applicationConfigurationView.XpPerMinuteWinner = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				applicationConfigurationView.XpRequiredPerLevel = DictionaryProxy<int, int>.Deserialize(bytes, new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize), new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize));
			}
			applicationConfigurationView.XpSmackdown = Int32Proxy.Deserialize(bytes);
			return applicationConfigurationView;
		}
	}
}
