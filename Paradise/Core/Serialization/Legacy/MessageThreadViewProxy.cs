using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class MessageThreadViewProxy
	{
		public static void Serialize(Stream stream, MessageThreadView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					BooleanProxy.Serialize(memoryStream, instance.HasNewMessages);
					if (instance.LastMessagePreview != null)
					{
						StringProxy.Serialize(memoryStream, instance.LastMessagePreview);
					}
					else
					{
						num |= 1;
					}
					DateTimeProxy.Serialize(memoryStream, instance.LastUpdate);
					Int32Proxy.Serialize(memoryStream, instance.MessageCount);
					Int32Proxy.Serialize(memoryStream, instance.ThreadId);
					if (instance.ThreadName != null)
					{
						StringProxy.Serialize(memoryStream, instance.ThreadName);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MessageThreadView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			MessageThreadView messageThreadView = null;
			if (num != 0)
			{
				messageThreadView = new MessageThreadView();
				messageThreadView.HasNewMessages = BooleanProxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					messageThreadView.LastMessagePreview = StringProxy.Deserialize(bytes);
				}
				messageThreadView.LastUpdate = DateTimeProxy.Deserialize(bytes);
				messageThreadView.MessageCount = Int32Proxy.Deserialize(bytes);
				messageThreadView.ThreadId = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0)
				{
					messageThreadView.ThreadName = StringProxy.Deserialize(bytes);
				}
			}
			return messageThreadView;
		}
	}
}
