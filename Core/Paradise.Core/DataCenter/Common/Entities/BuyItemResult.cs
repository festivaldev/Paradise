﻿namespace Paradise.DataCenter.Common.Entities {
	public enum BuyItemResult {
		OK,
		DisableInShop,
		DisableForRent = 3,
		DisableForPermanent,
		DurationDisabled,
		PackDisabled,
		IsNotForSale,
		NotEnoughCurrency,
		InvalidMember,
		InvalidExpirationDate,
		AlreadyInInventory,
		InvalidAmount,
		NoStockRemaining,
		InvalidData,
		TooManyUsage,
		InvalidLevel = 100,
		ItemNotFound = 404
	}
}
