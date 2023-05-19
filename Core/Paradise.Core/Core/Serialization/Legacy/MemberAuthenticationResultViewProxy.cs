using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class MemberAuthenticationResultViewProxy {
		public static void Serialize(Stream stream, MemberAuthenticationResultView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					BooleanProxy.Serialize(memoryStream, instance.IsAccountComplete);
					BooleanProxy.Serialize(memoryStream, instance.IsTutorialComplete);
					if (instance.LuckyDraw != null) {
						LuckyDrawUnityViewProxy.Serialize(memoryStream, instance.LuckyDraw);
					} else {
						num |= 1;
					}
					EnumProxy<MemberAuthenticationResult>.Serialize(memoryStream, instance.MemberAuthenticationResult);
					if (instance.MemberView != null) {
						MemberViewProxy.Serialize(memoryStream, instance.MemberView);
					} else {
						num |= 2;
					}
					if (instance.PlayerStatisticsView != null) {
						PlayerStatisticsViewProxy.Serialize(memoryStream, instance.PlayerStatisticsView);
					} else {
						num |= 4;
					}
					DateTimeProxy.Serialize(memoryStream, instance.ServerTime);
					if (instance.WeeklySpecial != null) {
						WeeklySpecialViewProxy.Serialize(memoryStream, instance.WeeklySpecial);
					} else {
						num |= 8;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MemberAuthenticationResultView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberAuthenticationResultView memberAuthenticationResultView = null;
			if (num != 0) {
				memberAuthenticationResultView = new MemberAuthenticationResultView();
				memberAuthenticationResultView.IsAccountComplete = BooleanProxy.Deserialize(bytes);
				memberAuthenticationResultView.IsTutorialComplete = BooleanProxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					memberAuthenticationResultView.LuckyDraw = LuckyDrawUnityViewProxy.Deserialize(bytes);
				}
				memberAuthenticationResultView.MemberAuthenticationResult = EnumProxy<MemberAuthenticationResult>.Deserialize(bytes);
				if ((num & 2) != 0) {
					memberAuthenticationResultView.MemberView = MemberViewProxy.Deserialize(bytes);
				}
				if ((num & 4) != 0) {
					memberAuthenticationResultView.PlayerStatisticsView = PlayerStatisticsViewProxy.Deserialize(bytes);
				}
				memberAuthenticationResultView.ServerTime = DateTimeProxy.Deserialize(bytes);
				if ((num & 8) != 0) {
					memberAuthenticationResultView.WeeklySpecial = WeeklySpecialViewProxy.Deserialize(bytes);
				}
			}
			return memberAuthenticationResultView;
		}
	}
}
