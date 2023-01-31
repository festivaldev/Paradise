using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IApplicationWebServiceContract : IWebServiceContractBase {
		[OperationContract]
		byte[] AuthenticateApplication(byte[] data);

		[OperationContract]
		byte[] GetConfigurationData(byte[] data);

		[OperationContract]
		byte[] GetMaps(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] GetCustomMaps(byte[] data);

		[OperationContract]
		byte[] SetMatchScore(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] PublishCommMonitoringData(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] PublishGameMonitoringData(byte[] data);
	}
}