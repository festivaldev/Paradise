using System.ServiceModel;

namespace Paradise {
	[ServiceContract]
	public interface IParadiseMonitoring {
		[OperationContract]
		string GetStatus();

		[OperationContract]
		byte Ping();
	}
}
