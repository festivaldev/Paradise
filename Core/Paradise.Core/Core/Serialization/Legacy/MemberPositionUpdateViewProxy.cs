using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class MemberPositionUpdateViewProxy
	{
		public static void Serialize(Stream stream, MemberPositionUpdateView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.CmidMakingAction);
					Int32Proxy.Serialize(memoryStream, instance.GroupId);
					Int32Proxy.Serialize(memoryStream, instance.MemberCmid);
					EnumProxy<GroupPosition>.Serialize(memoryStream, instance.Position);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MemberPositionUpdateView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			MemberPositionUpdateView memberPositionUpdateView = null;
			if (num != 0)
			{
				memberPositionUpdateView = new MemberPositionUpdateView();
				memberPositionUpdateView.CmidMakingAction = Int32Proxy.Deserialize(bytes);
				memberPositionUpdateView.GroupId = Int32Proxy.Deserialize(bytes);
				memberPositionUpdateView.MemberCmid = Int32Proxy.Deserialize(bytes);
				memberPositionUpdateView.Position = EnumProxy<GroupPosition>.Deserialize(bytes);
			}
			return memberPositionUpdateView;
		}
	}
}
