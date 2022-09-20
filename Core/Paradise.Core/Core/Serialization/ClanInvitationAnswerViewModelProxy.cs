using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ClanInvitationAnswerViewModelProxy {
		public static void Serialize(Stream stream, ClanInvitationAnswerViewModel instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.GroupInvitationId);
				BooleanProxy.Serialize(memoryStream, instance.IsInvitationAccepted);
				Int32Proxy.Serialize(memoryStream, instance.ReturnValue);
				memoryStream.WriteTo(stream);
			}
		}

		public static ClanInvitationAnswerViewModel Deserialize(Stream bytes) {
			return new ClanInvitationAnswerViewModel {
				GroupInvitationId = Int32Proxy.Deserialize(bytes),
				IsInvitationAccepted = BooleanProxy.Deserialize(bytes),
				ReturnValue = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
