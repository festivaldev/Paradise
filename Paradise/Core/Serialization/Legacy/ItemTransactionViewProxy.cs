using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ItemTransactionViewProxy
	{
		public static void Serialize(Stream stream, ItemTransactionView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					Int32Proxy.Serialize(memoryStream, instance.Credits);
					EnumProxy<BuyingDurationType>.Serialize(memoryStream, instance.Duration);
					BooleanProxy.Serialize(memoryStream, instance.IsAdminAction);
					Int32Proxy.Serialize(memoryStream, instance.ItemId);
					Int32Proxy.Serialize(memoryStream, instance.Points);
					DateTimeProxy.Serialize(memoryStream, instance.WithdrawalDate);
					Int32Proxy.Serialize(memoryStream, instance.WithdrawalId);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ItemTransactionView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			ItemTransactionView itemTransactionView = null;
			if (num != 0)
			{
				itemTransactionView = new ItemTransactionView();
				itemTransactionView.Cmid = Int32Proxy.Deserialize(bytes);
				itemTransactionView.Credits = Int32Proxy.Deserialize(bytes);
				itemTransactionView.Duration = EnumProxy<BuyingDurationType>.Deserialize(bytes);
				itemTransactionView.IsAdminAction = BooleanProxy.Deserialize(bytes);
				itemTransactionView.ItemId = Int32Proxy.Deserialize(bytes);
				itemTransactionView.Points = Int32Proxy.Deserialize(bytes);
				itemTransactionView.WithdrawalDate = DateTimeProxy.Deserialize(bytes);
				itemTransactionView.WithdrawalId = Int32Proxy.Deserialize(bytes);
			}
			return itemTransactionView;
		}
	}
}
