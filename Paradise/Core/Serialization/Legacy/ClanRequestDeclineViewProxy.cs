using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ClanRequestDeclineViewProxy
	{
		public static void Serialize(Stream stream, ClanRequestDeclineView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.ActionResult);
					Int32Proxy.Serialize(memoryStream, instance.ClanRequestId);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ClanRequestDeclineView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			ClanRequestDeclineView clanRequestDeclineView = null;
			if (num != 0)
			{
				clanRequestDeclineView = new ClanRequestDeclineView();
				clanRequestDeclineView.ActionResult = Int32Proxy.Deserialize(bytes);
				clanRequestDeclineView.ClanRequestId = Int32Proxy.Deserialize(bytes);
			}
			return clanRequestDeclineView;
		}
	}
}
