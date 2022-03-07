﻿using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ItemTransactionsViewModelProxy {
		public static void Serialize(Stream stream, ItemTransactionsViewModel instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.ItemTransactions != null) {
					ListProxy<ItemTransactionView>.Serialize(memoryStream, instance.ItemTransactions, new ListProxy<ItemTransactionView>.Serializer<ItemTransactionView>(ItemTransactionViewProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.TotalCount);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ItemTransactionsViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ItemTransactionsViewModel itemTransactionsViewModel = new ItemTransactionsViewModel();
			if ((num & 1) != 0) {
				itemTransactionsViewModel.ItemTransactions = ListProxy<ItemTransactionView>.Deserialize(bytes, new ListProxy<ItemTransactionView>.Deserializer<ItemTransactionView>(ItemTransactionViewProxy.Deserialize));
			}
			itemTransactionsViewModel.TotalCount = Int32Proxy.Deserialize(bytes);
			return itemTransactionsViewModel;
		}
	}
}
