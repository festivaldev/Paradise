﻿using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class LuckyDrawUnityView {
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int Price { get; set; }

		public UberStrikeCurrencyType UberStrikeCurrencyType { get; set; }

		public string IconUrl { get; set; }

		public BundleCategoryType Category { get; set; }

		public bool IsAvailableInShop { get; set; }

		public List<LuckyDrawSetUnityView> LuckyDrawSets { get; set; }
	}
}
