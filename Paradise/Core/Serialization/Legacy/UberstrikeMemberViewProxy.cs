using System;
using System.IO;
using Paradise.DataCenter.Common.Entities;

namespace Paradise.Core.Serialization.Legacy
{
	public static class UberstrikeMemberViewProxy
	{
		public static void Serialize(Stream stream, UberstrikeMemberView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					if (instance.PlayerCardView != null)
					{
						PlayerCardViewProxy.Serialize(memoryStream, instance.PlayerCardView);
					}
					else
					{
						num |= 1;
					}
					if (instance.PlayerStatisticsView != null)
					{
						PlayerStatisticsViewProxy.Serialize(memoryStream, instance.PlayerStatisticsView);
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

		public static UberstrikeMemberView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			UberstrikeMemberView uberstrikeMemberView = null;
			if (num != 0)
			{
				uberstrikeMemberView = new UberstrikeMemberView();
				if ((num & 1) != 0)
				{
					uberstrikeMemberView.PlayerCardView = PlayerCardViewProxy.Deserialize(bytes);
				}
				if ((num & 2) != 0)
				{
					uberstrikeMemberView.PlayerStatisticsView = PlayerStatisticsViewProxy.Deserialize(bytes);
				}
			}
			return uberstrikeMemberView;
		}
	}
}
