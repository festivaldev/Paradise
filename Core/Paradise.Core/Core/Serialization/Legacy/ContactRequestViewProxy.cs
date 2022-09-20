using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ContactRequestViewProxy
	{
		public static void Serialize(Stream stream, ContactRequestView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.InitiatorCmid);
					if (instance.InitiatorMessage != null)
					{
						StringProxy.Serialize(memoryStream, instance.InitiatorMessage);
					}
					else
					{
						num |= 1;
					}
					if (instance.InitiatorName != null)
					{
						StringProxy.Serialize(memoryStream, instance.InitiatorName);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.ReceiverCmid);
					Int32Proxy.Serialize(memoryStream, instance.RequestId);
					DateTimeProxy.Serialize(memoryStream, instance.SentDate);
					EnumProxy<ContactRequestStatus>.Serialize(memoryStream, instance.Status);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ContactRequestView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			ContactRequestView contactRequestView = null;
			if (num != 0)
			{
				contactRequestView = new ContactRequestView();
				contactRequestView.InitiatorCmid = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					contactRequestView.InitiatorMessage = StringProxy.Deserialize(bytes);
				}
				if ((num & 2) != 0)
				{
					contactRequestView.InitiatorName = StringProxy.Deserialize(bytes);
				}
				contactRequestView.ReceiverCmid = Int32Proxy.Deserialize(bytes);
				contactRequestView.RequestId = Int32Proxy.Deserialize(bytes);
				contactRequestView.SentDate = DateTimeProxy.Deserialize(bytes);
				contactRequestView.Status = EnumProxy<ContactRequestStatus>.Deserialize(bytes);
			}
			return contactRequestView;
		}
	}
}
