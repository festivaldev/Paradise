using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PrivateMessageViewProxy {
		public static void Serialize(Stream stream, PrivateMessageView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.ContentText != null) {
					StringProxy.Serialize(memoryStream, instance.ContentText);
				} else {
					num |= 1;
				}
				DateTimeProxy.Serialize(memoryStream, instance.DateSent);
				Int32Proxy.Serialize(memoryStream, instance.FromCmid);
				if (instance.FromName != null) {
					StringProxy.Serialize(memoryStream, instance.FromName);
				} else {
					num |= 2;
				}
				BooleanProxy.Serialize(memoryStream, instance.HasAttachment);
				BooleanProxy.Serialize(memoryStream, instance.IsDeletedByReceiver);
				BooleanProxy.Serialize(memoryStream, instance.IsDeletedBySender);
				BooleanProxy.Serialize(memoryStream, instance.IsRead);
				Int32Proxy.Serialize(memoryStream, instance.PrivateMessageId);
				Int32Proxy.Serialize(memoryStream, instance.ToCmid);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PrivateMessageView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PrivateMessageView privateMessageView = new PrivateMessageView();
			if ((num & 1) != 0) {
				privateMessageView.ContentText = StringProxy.Deserialize(bytes);
			}
			privateMessageView.DateSent = DateTimeProxy.Deserialize(bytes);
			privateMessageView.FromCmid = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				privateMessageView.FromName = StringProxy.Deserialize(bytes);
			}
			privateMessageView.HasAttachment = BooleanProxy.Deserialize(bytes);
			privateMessageView.IsDeletedByReceiver = BooleanProxy.Deserialize(bytes);
			privateMessageView.IsDeletedBySender = BooleanProxy.Deserialize(bytes);
			privateMessageView.IsRead = BooleanProxy.Deserialize(bytes);
			privateMessageView.PrivateMessageId = Int32Proxy.Deserialize(bytes);
			privateMessageView.ToCmid = Int32Proxy.Deserialize(bytes);
			return privateMessageView;
		}
	}
}
