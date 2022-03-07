using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ContactRequestAcceptViewProxy
	{
		//public static void Serialize(Stream stream, ContactRequestAcceptView instance)
		//{
		//	int num = 0;
		//	if (instance != null)
		//	{
		//		using (MemoryStream memoryStream = new MemoryStream())
		//		{
		//			Int32Proxy.Serialize(memoryStream, instance.ActionResult);
		//			if (instance.Contact != null)
		//			{
		//				PublicProfileViewProxy.Serialize(memoryStream, instance.Contact);
		//			}
		//			else
		//			{
		//				num |= 1;
		//			}
		//			Int32Proxy.Serialize(memoryStream, instance.RequestId);
		//			Int32Proxy.Serialize(stream, ~num);
		//			memoryStream.WriteTo(stream);
		//		}
		//	}
		//	else
		//	{
		//		Int32Proxy.Serialize(stream, 0);
		//	}
		//}

		//public static ContactRequestAcceptView Deserialize(Stream bytes)
		//{
		//	int num = Int32Proxy.Deserialize(bytes);
		//	ContactRequestAcceptView contactRequestAcceptView = null;
		//	if (num != 0)
		//	{
		//		contactRequestAcceptView = new ContactRequestAcceptView();
		//		contactRequestAcceptView.ActionResult = Int32Proxy.Deserialize(bytes);
		//		if ((num & 1) != 0)
		//		{
		//			contactRequestAcceptView.Contact = PublicProfileViewProxy.Deserialize(bytes);
		//		}
		//		contactRequestAcceptView.RequestId = Int32Proxy.Deserialize(bytes);
		//	}
		//	return contactRequestAcceptView;
		//}
	}
}
