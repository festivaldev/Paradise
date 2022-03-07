using System.Collections.Generic;
using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class ItemView {
		public ItemView() {
		}

		protected ItemView(ItemView itemView) {
			this.AmountRemaining = itemView.AmountRemaining;
			this.ClassId = itemView.ClassId;
			this.CreditsPerDay = itemView.CreditsPerDay;
			this.Description = itemView.Description;
			this.IsFeatured = itemView.IsFeatured;
			this.IsForSale = itemView.IsForSale;
			this.IsNew = itemView.IsNew;
			this.IsPopular = itemView.IsPopular;
			this.ItemId = itemView.ItemId;
			this.Name = itemView.Name;
			this.PrefabName = itemView.PrefabName;
			this.PermanentCredits = itemView.PermanentCredits;
			this.PointsPerDay = itemView.PointsPerDay;
			this.PurchaseType = itemView.PurchaseType;
			this.TypeId = itemView.TypeId;
			this.PackOneAmount = itemView.PackOneAmount;
			this.PackTwoAmount = itemView.PackTwoAmount;
			this.PackThreeAmount = itemView.PackThreeAmount;
			this.MaximumOwnableAmount = itemView.MaximumOwnableAmount;
			this.Enable1Day = itemView.Enable1Day;
			this.Enable7Days = itemView.Enable7Days;
			this.Enable30Days = itemView.Enable30Days;
			this.Enable90Days = itemView.Enable90Days;
			this.MaximumDurationDays = itemView.MaximumDurationDays;
			this.PermanentPoints = itemView.PermanentPoints;
			this.IsDisable = itemView.IsDisable;
			this.CustomProperties = ((itemView.CustomProperties == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(itemView.CustomProperties));
			this.ItemProperties = ((this.ItemProperties == null) ? new Dictionary<ItemPropertyType, int>() : new Dictionary<ItemPropertyType, int>(itemView.ItemProperties));
		}

		public int ItemId { get; set; }

		public string Name { get; set; }

		public string PrefabName { get; set; }

		public string Description { get; set; }

		public int CreditsPerDay { get; set; }

		public int PointsPerDay { get; set; }

		public int PermanentPoints { get; set; }

		public int PermanentCredits { get; set; }

		public bool IsDisable { get; set; }

		public bool IsForSale { get; set; }

		public bool IsNew { get; set; }

		public bool IsPopular { get; set; }

		public bool IsFeatured { get; set; }

		public PurchaseType PurchaseType { get; set; }

		public int TypeId { get; set; }

		public int ClassId { get; set; }

		public int AmountRemaining { get; set; }

		public int PackOneAmount { get; set; }

		public int PackTwoAmount { get; set; }

		public int PackThreeAmount { get; set; }

		public bool Enable1Day { get; set; }

		public bool Enable7Days { get; set; }

		public bool Enable30Days { get; set; }

		public bool Enable90Days { get; set; }

		public int MaximumDurationDays { get; set; }

		public int MaximumOwnableAmount { get; set; }

		public Dictionary<string, string> CustomProperties { get; set; }

		public Dictionary<ItemPropertyType, int> ItemProperties { get; set; }

		public bool EnablePermanent {
			get {
				return this.PermanentCredits != -1 || this.PermanentPoints != -1;
			}
		}

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[ItemView: [AmountRemaining: ");
			stringBuilder.Append(this.AmountRemaining);
			stringBuilder.Append("][ClassId: ");
			stringBuilder.Append(this.ClassId);
			stringBuilder.Append("][CreditsPerDayShop: ");
			stringBuilder.Append(this.CreditsPerDay);
			stringBuilder.Append("][Description: ");
			stringBuilder.Append(this.Description);
			stringBuilder.Append("][IsFeatured: ");
			stringBuilder.Append(this.IsFeatured);
			stringBuilder.Append("][IsForSale: ");
			stringBuilder.Append(this.IsForSale);
			stringBuilder.Append("][IsNew: ");
			stringBuilder.Append(this.IsNew);
			stringBuilder.Append("][IsPopular: ");
			stringBuilder.Append(this.IsPopular);
			stringBuilder.Append("][ItemId: ");
			stringBuilder.Append(this.ItemId);
			stringBuilder.Append("][Name: ");
			stringBuilder.Append(this.Name);
			stringBuilder.Append("][PrefabName: ");
			stringBuilder.Append(this.PrefabName);
			stringBuilder.Append("][PermanentCredits: ");
			stringBuilder.Append(this.PermanentCredits);
			stringBuilder.Append("][PointsPerDayShop: ");
			stringBuilder.Append(this.PointsPerDay);
			stringBuilder.Append("][PurchaseType: ");
			stringBuilder.Append(this.PurchaseType);
			stringBuilder.Append("][TypeId: ");
			stringBuilder.Append(this.TypeId);
			stringBuilder.Append("][PackOneAmount: ");
			stringBuilder.Append(this.PackOneAmount);
			stringBuilder.Append("][PackTwoAmount: ");
			stringBuilder.Append(this.PackTwoAmount);
			stringBuilder.Append("][PackThreeAmount: ");
			stringBuilder.Append(this.PackThreeAmount);
			stringBuilder.Append("][MaximumOwnableAmount: ");
			stringBuilder.Append(this.MaximumOwnableAmount);
			stringBuilder.Append("][Enable1Day: ");
			stringBuilder.Append(this.Enable1Day);
			stringBuilder.Append("][Enable7Days: ");
			stringBuilder.Append(this.Enable7Days);
			stringBuilder.Append("][Enable30Days: ");
			stringBuilder.Append(this.Enable30Days);
			stringBuilder.Append("][Enable90Days: ");
			stringBuilder.Append(this.Enable90Days);
			stringBuilder.Append("][MaximumDurationDays: ");
			stringBuilder.Append(this.MaximumDurationDays);
			stringBuilder.Append("][PermanentPoints: ");
			stringBuilder.Append(this.PermanentPoints);
			stringBuilder.Append("][IsDisable: ");
			stringBuilder.Append(this.IsDisable);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
