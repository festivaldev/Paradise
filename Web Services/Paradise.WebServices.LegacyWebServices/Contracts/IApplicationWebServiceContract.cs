using System.ServiceModel;

namespace Paradise.WebServices.LegacyWebServices {
	[ServiceContract]
	public interface IApplicationWebServiceContract {
		[OperationContract]
		byte[] GetMyIP(byte[] data);

		[OperationContract]
		byte[] AuthenticateApplication(byte[] data);

		[OperationContract]
		byte[] RecordException(byte[] data);

		[OperationContract]
		byte[] RecordExceptionUnencrypted(byte[] data);

		[OperationContract]
		byte[] RecordTutorialStep(byte[] data);

		[OperationContract]
		byte[] ReportBug(byte[] data);

		[OperationContract]
		byte[] GetLiveFeed(byte[] data);

		[OperationContract]
		byte[] GetMaps(byte[] data);

		[OperationContract]
		byte[] GetItemAssetBundles(byte[] data);

		[OperationContract]
		byte[] SetLevelVersion(byte[] data);

		[OperationContract]
		byte[] GetPhotonServerName(byte[] data);

		[OperationContract]
		byte[] IsAlive(byte[] data);
	}
}
