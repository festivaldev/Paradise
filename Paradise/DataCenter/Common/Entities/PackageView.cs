using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PackageView {
		public PackageView() {
			this.Bonus = 0;
			this.Price = 0m;
			this.Items = new List<int>();
			this.Name = string.Empty;
		}

		public PackageView(int bonus, decimal price, List<int> items, string name) {
			this.Bonus = bonus;
			this.Price = price;
			this.Items = items;
			this.Name = name;
		}

		public int Bonus { get; set; }

		public decimal Price { get; set; }

		public List<int> Items { get; set; }

		public string Name { get; set; }
	}
}
