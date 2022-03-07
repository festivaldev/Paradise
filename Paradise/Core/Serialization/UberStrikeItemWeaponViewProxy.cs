using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class UberStrikeItemWeaponViewProxy {
		public static void Serialize(Stream stream, UberStrikeItemWeaponView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.AccuracySpread);
				Int32Proxy.Serialize(memoryStream, instance.CombatRange);
				Int32Proxy.Serialize(memoryStream, instance.CriticalStrikeBonus);
				if (instance.CustomProperties != null) {
					DictionaryProxy<string, string>.Serialize(memoryStream, instance.CustomProperties, new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize), new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.DamageKnockback);
				Int32Proxy.Serialize(memoryStream, instance.DamagePerProjectile);
				Int32Proxy.Serialize(memoryStream, instance.DefaultZoomMultiplier);
				if (instance.Description != null) {
					StringProxy.Serialize(memoryStream, instance.Description);
				} else {
					num |= 2;
				}
				BooleanProxy.Serialize(memoryStream, instance.HasAutomaticFire);
				Int32Proxy.Serialize(memoryStream, instance.ID);
				BooleanProxy.Serialize(memoryStream, instance.IsConsumable);
				EnumProxy<UberstrikeItemClass>.Serialize(memoryStream, instance.ItemClass);
				if (instance.ItemProperties != null) {
					DictionaryProxy<ItemPropertyType, int>.Serialize(memoryStream, instance.ItemProperties, new DictionaryProxy<ItemPropertyType, int>.Serializer<ItemPropertyType>(EnumProxy<ItemPropertyType>.Serialize), new DictionaryProxy<ItemPropertyType, int>.Serializer<int>(Int32Proxy.Serialize));
				} else {
					num |= 4;
				}
				Int32Proxy.Serialize(memoryStream, instance.LevelLock);
				Int32Proxy.Serialize(memoryStream, instance.MaxAmmo);
				Int32Proxy.Serialize(memoryStream, instance.MaxDurationDays);
				Int32Proxy.Serialize(memoryStream, instance.MaxZoomMultiplier);
				Int32Proxy.Serialize(memoryStream, instance.MinZoomMultiplier);
				Int32Proxy.Serialize(memoryStream, instance.MissileBounciness);
				Int32Proxy.Serialize(memoryStream, instance.MissileForceImpulse);
				Int32Proxy.Serialize(memoryStream, instance.MissileTimeToDetonate);
				if (instance.Name != null) {
					StringProxy.Serialize(memoryStream, instance.Name);
				} else {
					num |= 8;
				}
				if (instance.PrefabName != null) {
					StringProxy.Serialize(memoryStream, instance.PrefabName);
				} else {
					num |= 16;
				}
				if (instance.Prices != null) {
					ListProxy<ItemPrice>.Serialize(memoryStream, instance.Prices, new ListProxy<ItemPrice>.Serializer<ItemPrice>(ItemPriceProxy.Serialize));
				} else {
					num |= 32;
				}
				Int32Proxy.Serialize(memoryStream, instance.ProjectileSpeed);
				Int32Proxy.Serialize(memoryStream, instance.ProjectilesPerShot);
				Int32Proxy.Serialize(memoryStream, instance.RateOfFire);
				Int32Proxy.Serialize(memoryStream, instance.RecoilKickback);
				Int32Proxy.Serialize(memoryStream, instance.RecoilMovement);
				Int32Proxy.Serialize(memoryStream, instance.SecondaryActionReticle);
				EnumProxy<ItemShopHighlightType>.Serialize(memoryStream, instance.ShopHighlightType);
				Int32Proxy.Serialize(memoryStream, instance.SplashRadius);
				Int32Proxy.Serialize(memoryStream, instance.StartAmmo);
				Int32Proxy.Serialize(memoryStream, instance.Tier);
				Int32Proxy.Serialize(memoryStream, instance.WeaponSecondaryAction);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static UberStrikeItemWeaponView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemWeaponView uberStrikeItemWeaponView = new UberStrikeItemWeaponView();
			uberStrikeItemWeaponView.AccuracySpread = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.CombatRange = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.CriticalStrikeBonus = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				uberStrikeItemWeaponView.CustomProperties = DictionaryProxy<string, string>.Deserialize(bytes, new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize), new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize));
			}
			uberStrikeItemWeaponView.DamageKnockback = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.DamagePerProjectile = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.DefaultZoomMultiplier = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				uberStrikeItemWeaponView.Description = StringProxy.Deserialize(bytes);
			}
			uberStrikeItemWeaponView.HasAutomaticFire = BooleanProxy.Deserialize(bytes);
			uberStrikeItemWeaponView.ID = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.IsConsumable = BooleanProxy.Deserialize(bytes);
			uberStrikeItemWeaponView.ItemClass = EnumProxy<UberstrikeItemClass>.Deserialize(bytes);
			if ((num & 4) != 0) {
				uberStrikeItemWeaponView.ItemProperties = DictionaryProxy<ItemPropertyType, int>.Deserialize(bytes, new DictionaryProxy<ItemPropertyType, int>.Deserializer<ItemPropertyType>(EnumProxy<ItemPropertyType>.Deserialize), new DictionaryProxy<ItemPropertyType, int>.Deserializer<int>(Int32Proxy.Deserialize));
			}
			uberStrikeItemWeaponView.LevelLock = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MaxAmmo = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MaxDurationDays = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MaxZoomMultiplier = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MinZoomMultiplier = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MissileBounciness = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MissileForceImpulse = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.MissileTimeToDetonate = Int32Proxy.Deserialize(bytes);
			if ((num & 8) != 0) {
				uberStrikeItemWeaponView.Name = StringProxy.Deserialize(bytes);
			}
			if ((num & 16) != 0) {
				uberStrikeItemWeaponView.PrefabName = StringProxy.Deserialize(bytes);
			}
			if ((num & 32) != 0) {
				uberStrikeItemWeaponView.Prices = ListProxy<ItemPrice>.Deserialize(bytes, new ListProxy<ItemPrice>.Deserializer<ItemPrice>(ItemPriceProxy.Deserialize));
			}
			uberStrikeItemWeaponView.ProjectileSpeed = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.ProjectilesPerShot = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.RateOfFire = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.RecoilKickback = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.RecoilMovement = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.SecondaryActionReticle = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.ShopHighlightType = EnumProxy<ItemShopHighlightType>.Deserialize(bytes);
			uberStrikeItemWeaponView.SplashRadius = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.StartAmmo = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.Tier = Int32Proxy.Deserialize(bytes);
			uberStrikeItemWeaponView.WeaponSecondaryAction = Int32Proxy.Deserialize(bytes);
			return uberStrikeItemWeaponView;
		}
	}
}
