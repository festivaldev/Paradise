using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ClanInvitationAnswerViewModelProxy {
		public static void Serialize(Stream stream, ClanInvitationAnswerViewModel instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.GroupInvitationId);
					BooleanProxy.Serialize(memoryStream, instance.IsInvitationAccepted);
					Int32Proxy.Serialize(memoryStream, instance.ReturnValue);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ClanInvitationAnswerViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ClanInvitationAnswerViewModel clanInvitationAnswerViewModel = null;
			if (num != 0) {
				clanInvitationAnswerViewModel = new ClanInvitationAnswerViewModel();
				clanInvitationAnswerViewModel.GroupInvitationId = Int32Proxy.Deserialize(bytes);
				clanInvitationAnswerViewModel.IsInvitationAccepted = BooleanProxy.Deserialize(bytes);
				clanInvitationAnswerViewModel.ReturnValue = Int32Proxy.Deserialize(bytes);
			}
			return clanInvitationAnswerViewModel;
		}
	}
}
