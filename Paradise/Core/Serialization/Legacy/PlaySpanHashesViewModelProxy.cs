using System;
using System.IO;
using Paradise.Core.ViewModel;

namespace Paradise.Core.Serialization.Legacy
{
	public static class PlaySpanHashesViewModelProxy
	{
		public static void Serialize(Stream stream, PlaySpanHashesViewModel instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					if (instance.Hashes != null)
					{
						DictionaryProxy<decimal, string>.Serialize(memoryStream, instance.Hashes, new DictionaryProxy<decimal, string>.Serializer<decimal>(DecimalProxy.Serialize), new DictionaryProxy<decimal, string>.Serializer<string>(StringProxy.Serialize));
					}
					else
					{
						num |= 1;
					}
					if (instance.MerchTrans != null)
					{
						StringProxy.Serialize(memoryStream, instance.MerchTrans);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PlaySpanHashesViewModel Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			PlaySpanHashesViewModel playSpanHashesViewModel = null;
			if (num != 0)
			{
				playSpanHashesViewModel = new PlaySpanHashesViewModel();
				if ((num & 1) != 0)
				{
					playSpanHashesViewModel.Hashes = DictionaryProxy<decimal, string>.Deserialize(bytes, new DictionaryProxy<decimal, string>.Deserializer<decimal>(DecimalProxy.Deserialize), new DictionaryProxy<decimal, string>.Deserializer<string>(StringProxy.Deserialize));
				}
				if ((num & 2) != 0)
				{
					playSpanHashesViewModel.MerchTrans = StringProxy.Deserialize(bytes);
				}
			}
			return playSpanHashesViewModel;
		}
	}
}
