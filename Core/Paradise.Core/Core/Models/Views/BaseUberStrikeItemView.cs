using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Core.Models.Views {
	public abstract class BaseUberStrikeItemView {
		public abstract UberstrikeItemType ItemType { get; }

		public UberstrikeItemClass ItemClass {
			get {
				return this._itemClass;
			}
			set {
				this._itemClass = value;
			}
		}

		public int ID { get; set; }

		public string Name { get; set; }

		public string PrefabName { get; set; }

		public string Description { get; set; }

		public int LevelLock { get; set; }

		public int MaxDurationDays { get; set; }

		public bool IsConsumable { get; set; }

		public ICollection<ItemPrice> Prices { get; set; }

		public bool IsForSale {
			get {
				return this.Prices != null && this.Prices.Count > 0;
			}
		}

		public ItemShopHighlightType ShopHighlightType { get; set; }

		public Dictionary<string, string> CustomProperties { get; set; }

		public Dictionary<ItemPropertyType, int> ItemProperties { get; set; }

		[SerializeField]
		private UberstrikeItemClass _itemClass;
	}
}
