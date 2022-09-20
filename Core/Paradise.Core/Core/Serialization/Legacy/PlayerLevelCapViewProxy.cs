using System;
using System.IO;
using Paradise.DataCenter.Common.Entities;

namespace Paradise.Core.Serialization.Legacy
{
	public static class PlayerLevelCapViewProxy
	{
		public static void Serialize(Stream stream, PlayerLevelCapView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.Level);
					Int32Proxy.Serialize(memoryStream, instance.PlayerLevelCapId);
					Int32Proxy.Serialize(memoryStream, instance.XPRequired);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PlayerLevelCapView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			PlayerLevelCapView playerLevelCapView = null;
			if (num != 0)
			{
				playerLevelCapView = new PlayerLevelCapView();
				playerLevelCapView.Level = Int32Proxy.Deserialize(bytes);
				playerLevelCapView.PlayerLevelCapId = Int32Proxy.Deserialize(bytes);
				playerLevelCapView.XPRequired = Int32Proxy.Deserialize(bytes);
			}
			return playerLevelCapView;
		}
	}
}
