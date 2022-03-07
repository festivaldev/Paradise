using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ItemInventoryViewProxy
	{
		public static void Serialize(Stream stream, ItemInventoryView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.AmountRemaining);
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					if (instance.ExpirationDate != null)
					{
						DateTimeProxy.Serialize(memoryStream, instance.ExpirationDate ?? default(DateTime));
					}
					else
					{
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.ItemId);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ItemInventoryView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			ItemInventoryView itemInventoryView = null;
			if (num != 0)
			{
				itemInventoryView = new ItemInventoryView();
				itemInventoryView.AmountRemaining = Int32Proxy.Deserialize(bytes);
				itemInventoryView.Cmid = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					itemInventoryView.ExpirationDate = new DateTime?(DateTimeProxy.Deserialize(bytes));
				}
				itemInventoryView.ItemId = Int32Proxy.Deserialize(bytes);
			}
			return itemInventoryView;
		}
	}
}
