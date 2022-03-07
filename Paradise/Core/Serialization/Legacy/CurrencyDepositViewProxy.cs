using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class CurrencyDepositViewProxy
	{
		public static void Serialize(Stream stream, CurrencyDepositView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.ApplicationId);
					if (instance.BundleId != null)
					{
						Int32Proxy.Serialize(memoryStream, instance.BundleId ?? 0);
					}
					else
					{
						num |= 1;
					}
					if (instance.BundleName != null)
					{
						StringProxy.Serialize(memoryStream, instance.BundleName);
					}
					else
					{
						num |= 2;
					}
					DecimalProxy.Serialize(memoryStream, instance.Cash);
					EnumProxy<ChannelType>.Serialize(memoryStream, instance.ChannelId);
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					Int32Proxy.Serialize(memoryStream, instance.Credits);
					Int32Proxy.Serialize(memoryStream, instance.CreditsDepositId);
					if (instance.CurrencyLabel != null)
					{
						StringProxy.Serialize(memoryStream, instance.CurrencyLabel);
					}
					else
					{
						num |= 4;
					}
					DateTimeProxy.Serialize(memoryStream, instance.DepositDate);
					BooleanProxy.Serialize(memoryStream, instance.IsAdminAction);
					EnumProxy<PaymentProviderType>.Serialize(memoryStream, instance.PaymentProviderId);
					Int32Proxy.Serialize(memoryStream, instance.Points);
					if (instance.TransactionKey != null)
					{
						StringProxy.Serialize(memoryStream, instance.TransactionKey);
					}
					else
					{
						num |= 8;
					}
					DecimalProxy.Serialize(memoryStream, instance.UsdAmount);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static CurrencyDepositView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			CurrencyDepositView currencyDepositView = null;
			if (num != 0)
			{
				currencyDepositView = new CurrencyDepositView();
				currencyDepositView.ApplicationId = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					currencyDepositView.BundleId = new int?(Int32Proxy.Deserialize(bytes));
				}
				if ((num & 2) != 0)
				{
					currencyDepositView.BundleName = StringProxy.Deserialize(bytes);
				}
				currencyDepositView.Cash = DecimalProxy.Deserialize(bytes);
				currencyDepositView.ChannelId = EnumProxy<ChannelType>.Deserialize(bytes);
				currencyDepositView.Cmid = Int32Proxy.Deserialize(bytes);
				currencyDepositView.Credits = Int32Proxy.Deserialize(bytes);
				currencyDepositView.CreditsDepositId = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0)
				{
					currencyDepositView.CurrencyLabel = StringProxy.Deserialize(bytes);
				}
				currencyDepositView.DepositDate = DateTimeProxy.Deserialize(bytes);
				currencyDepositView.IsAdminAction = BooleanProxy.Deserialize(bytes);
				currencyDepositView.PaymentProviderId = EnumProxy<PaymentProviderType>.Deserialize(bytes);
				currencyDepositView.Points = Int32Proxy.Deserialize(bytes);
				if ((num & 8) != 0)
				{
					currencyDepositView.TransactionKey = StringProxy.Deserialize(bytes);
				}
				currencyDepositView.UsdAmount = DecimalProxy.Deserialize(bytes);
			}
			return currencyDepositView;
		}
	}
}
