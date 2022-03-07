using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ContactRequestDeclineViewProxy
	{
		//public static void Serialize(Stream stream, ContactRequestDeclineView instance)
		//{
		//	int num = 0;
		//	if (instance != null)
		//	{
		//		using (MemoryStream memoryStream = new MemoryStream())
		//		{
		//			Int32Proxy.Serialize(memoryStream, instance.ActionResult);
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

		//public static ContactRequestDeclineView Deserialize(Stream bytes)
		//{
		//	int num = Int32Proxy.Deserialize(bytes);
		//	ContactRequestDeclineView contactRequestDeclineView = null;
		//	if (num != 0)
		//	{
		//		contactRequestDeclineView = new ContactRequestDeclineView();
		//		contactRequestDeclineView.ActionResult = Int32Proxy.Deserialize(bytes);
		//		contactRequestDeclineView.RequestId = Int32Proxy.Deserialize(bytes);
		//	}
		//	return contactRequestDeclineView;
		//}
	}
}
