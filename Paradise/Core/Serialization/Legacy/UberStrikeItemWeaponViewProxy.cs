using System;
using System.IO;
using Paradise.Core.Models.Views;
using Paradise.Core.Types;

namespace Paradise.Core.Serialization.Legacy
{
	public static class UberStrikeItemWeaponViewProxy
	{
		public static void Serialize(Stream stream, UberStrikeItemWeaponView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.AccuracySpread);
					if (instance.CustomProperties != null)
					{
						DictionaryProxy<string, string>.Serialize(memoryStream, instance.CustomProperties, new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize), new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize));
					}
					else
					{
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.DamageKnockback);
					Int32Proxy.Serialize(memoryStream, instance.DamagePerProjectile);
					if (instance.Description != null)
					{
						StringProxy.Serialize(memoryStream, instance.Description);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.ID);
					BooleanProxy.Serialize(memoryStream, instance.IsConsumable);
					EnumProxy<UberstrikeItemClass>.Serialize(memoryStream, instance.ItemClass);
					Int32Proxy.Serialize(memoryStream, instance.LevelLock);
					Int32Proxy.Serialize(memoryStream, instance.MaxAmmo);
					Int32Proxy.Serialize(memoryStream, instance.MissileBounciness);
					Int32Proxy.Serialize(memoryStream, instance.MissileForceImpulse);
					Int32Proxy.Serialize(memoryStream, instance.MissileTimeToDetonate);
					if (instance.Name != null)
					{
						StringProxy.Serialize(memoryStream, instance.Name);
					}
					else
					{
						num |= 4;
					}
					if (instance.Prices != null)
					{
						ListProxy<ItemPrice>.Serialize(memoryStream, instance.Prices, new ListProxy<ItemPrice>.Serializer<ItemPrice>(ItemPriceProxy.Serialize));
					}
					else
					{
						num |= 8;
					}
					Int32Proxy.Serialize(memoryStream, instance.ProjectileSpeed);
					Int32Proxy.Serialize(memoryStream, instance.ProjectilesPerShot);
					Int32Proxy.Serialize(memoryStream, instance.RateOfFire);
					Int32Proxy.Serialize(memoryStream, instance.RecoilKickback);
					Int32Proxy.Serialize(memoryStream, instance.RecoilMovement);
					EnumProxy<ItemShopHighlightType>.Serialize(memoryStream, instance.ShopHighlightType);
					Int32Proxy.Serialize(memoryStream, instance.SplashRadius);
					Int32Proxy.Serialize(memoryStream, instance.StartAmmo);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static UberStrikeItemWeaponView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemWeaponView uberStrikeItemWeaponView = null;
			if (num != 0)
			{
				uberStrikeItemWeaponView = new UberStrikeItemWeaponView();
				uberStrikeItemWeaponView.AccuracySpread = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					uberStrikeItemWeaponView.CustomProperties = DictionaryProxy<string, string>.Deserialize(bytes, new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize), new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize));
				}
				uberStrikeItemWeaponView.DamageKnockback = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.DamagePerProjectile = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0)
				{
					uberStrikeItemWeaponView.Description = StringProxy.Deserialize(bytes);
				}
				uberStrikeItemWeaponView.ID = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.IsConsumable = BooleanProxy.Deserialize(bytes);
				uberStrikeItemWeaponView.ItemClass = EnumProxy<UberstrikeItemClass>.Deserialize(bytes);
				uberStrikeItemWeaponView.LevelLock = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.MaxAmmo = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.MissileBounciness = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.MissileForceImpulse = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.MissileTimeToDetonate = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0)
				{
					uberStrikeItemWeaponView.Name = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0)
				{
					uberStrikeItemWeaponView.Prices = ListProxy<ItemPrice>.Deserialize(bytes, new ListProxy<ItemPrice>.Deserializer<ItemPrice>(ItemPriceProxy.Deserialize));
				}
				uberStrikeItemWeaponView.ProjectileSpeed = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.ProjectilesPerShot = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.RateOfFire = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.RecoilKickback = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.RecoilMovement = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.ShopHighlightType = EnumProxy<ItemShopHighlightType>.Deserialize(bytes);
				uberStrikeItemWeaponView.SplashRadius = Int32Proxy.Deserialize(bytes);
				uberStrikeItemWeaponView.StartAmmo = Int32Proxy.Deserialize(bytes);
			}
			return uberStrikeItemWeaponView;
		}
	}
}
