using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class PointDepositViewProxy
	{
		public static void Serialize(Stream stream, PointDepositView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					DateTimeProxy.Serialize(memoryStream, instance.DepositDate);
					EnumProxy<PointsDepositType>.Serialize(memoryStream, instance.DepositType);
					BooleanProxy.Serialize(memoryStream, instance.IsAdminAction);
					Int32Proxy.Serialize(memoryStream, instance.PointDepositId);
					Int32Proxy.Serialize(memoryStream, instance.Points);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PointDepositView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			PointDepositView pointDepositView = null;
			if (num != 0)
			{
				pointDepositView = new PointDepositView();
				pointDepositView.Cmid = Int32Proxy.Deserialize(bytes);
				pointDepositView.DepositDate = DateTimeProxy.Deserialize(bytes);
				pointDepositView.DepositType = EnumProxy<PointsDepositType>.Deserialize(bytes);
				pointDepositView.IsAdminAction = BooleanProxy.Deserialize(bytes);
				pointDepositView.PointDepositId = Int32Proxy.Deserialize(bytes);
				pointDepositView.Points = Int32Proxy.Deserialize(bytes);
			}
			return pointDepositView;
		}
	}
}
