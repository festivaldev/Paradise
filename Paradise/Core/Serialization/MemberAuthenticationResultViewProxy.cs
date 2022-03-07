using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MemberAuthenticationResultViewProxy {
		public static void Serialize(Stream stream, MemberAuthenticationResultView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.AuthToken != null) {
					StringProxy.Serialize(memoryStream, instance.AuthToken);
				} else {
					num |= 1;
				}
				BooleanProxy.Serialize(memoryStream, instance.IsAccountComplete);
				if (instance.LuckyDraw != null) {
					LuckyDrawUnityViewProxy.Serialize(memoryStream, instance.LuckyDraw);
				} else {
					num |= 2;
				}
				EnumProxy<MemberAuthenticationResult>.Serialize(memoryStream, instance.MemberAuthenticationResult);
				if (instance.MemberView != null) {
					MemberViewProxy.Serialize(memoryStream, instance.MemberView);
				} else {
					num |= 4;
				}
				if (instance.PlayerStatisticsView != null) {
					PlayerStatisticsViewProxy.Serialize(memoryStream, instance.PlayerStatisticsView);
				} else {
					num |= 8;
				}
				DateTimeProxy.Serialize(memoryStream, instance.ServerTime);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MemberAuthenticationResultView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberAuthenticationResultView memberAuthenticationResultView = new MemberAuthenticationResultView();
			if ((num & 1) != 0) {
				memberAuthenticationResultView.AuthToken = StringProxy.Deserialize(bytes);
			}
			memberAuthenticationResultView.IsAccountComplete = BooleanProxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				memberAuthenticationResultView.LuckyDraw = LuckyDrawUnityViewProxy.Deserialize(bytes);
			}
			memberAuthenticationResultView.MemberAuthenticationResult = EnumProxy<MemberAuthenticationResult>.Deserialize(bytes);
			if ((num & 4) != 0) {
				memberAuthenticationResultView.MemberView = MemberViewProxy.Deserialize(bytes);
			}
			if ((num & 8) != 0) {
				memberAuthenticationResultView.PlayerStatisticsView = PlayerStatisticsViewProxy.Deserialize(bytes);
			}
			memberAuthenticationResultView.ServerTime = DateTimeProxy.Deserialize(bytes);
			return memberAuthenticationResultView;
		}
	}
}
