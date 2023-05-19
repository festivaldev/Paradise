using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class MemberReportViewProxy {
		public static void Serialize(Stream stream, MemberReportView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.ApplicationId);
					if (instance.Context != null) {
						StringProxy.Serialize(memoryStream, instance.Context);
					} else {
						num |= 1;
					}
					if (instance.IP != null) {
						StringProxy.Serialize(memoryStream, instance.IP);
					} else {
						num |= 2;
					}
					if (instance.Reason != null) {
						StringProxy.Serialize(memoryStream, instance.Reason);
					} else {
						num |= 4;
					}
					EnumProxy<MemberReportType>.Serialize(memoryStream, instance.ReportType);
					Int32Proxy.Serialize(memoryStream, instance.SourceCmid);
					Int32Proxy.Serialize(memoryStream, instance.TargetCmid);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MemberReportView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberReportView memberReportView = null;
			if (num != 0) {
				memberReportView = new MemberReportView();
				memberReportView.ApplicationId = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					memberReportView.Context = StringProxy.Deserialize(bytes);
				}
				if ((num & 2) != 0) {
					memberReportView.IP = StringProxy.Deserialize(bytes);
				}
				if ((num & 4) != 0) {
					memberReportView.Reason = StringProxy.Deserialize(bytes);
				}
				memberReportView.ReportType = EnumProxy<MemberReportType>.Deserialize(bytes);
				memberReportView.SourceCmid = Int32Proxy.Deserialize(bytes);
				memberReportView.TargetCmid = Int32Proxy.Deserialize(bytes);
			}
			return memberReportView;
		}
	}
}
