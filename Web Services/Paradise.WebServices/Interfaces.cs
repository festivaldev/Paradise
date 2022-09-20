using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Paradise.WebServices {
	internal enum RunMode {
		Console,
		Service,
		WinForms
	}

	public class ParadiseServiceStatus {
		public bool FileServerRunning { get; set; }
		public bool DatabaseOpened { get; set; }
		public List<Dictionary<string, object>> Services { get; set; }
	}

	public interface IParadiseServiceClient {
		//[OperationContract(IsOneWay = true)]
		//void OnError(string message, Exception e);

		[OperationContract(IsOneWay = true)]
		void OnError(string title, string message);

		[OperationContract(IsOneWay = true)]
		void SetTrayIconEnabled(bool enabled);

		[OperationContract(IsOneWay = true)]
		void OnDatabaseOpened();

		[OperationContract(IsOneWay = true)]
		void OnDatabaseClosed();

		[OperationContract(IsOneWay = true)]
		void OnDatabaseError();

		[OperationContract(IsOneWay = true)]
		void OnServiceStarted(ServiceEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnServiceStopped(ServiceEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnServiceError(ServiceEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnFileServerStarted();

		[OperationContract(IsOneWay = true)]
		void OnFileServerStopped();

		[OperationContract(IsOneWay = true)]
		void OnFileServerError(Exception e);

		[OperationContract(IsOneWay = true)]
		void OnConsoleCommandCallback(string message);
	}

	[ServiceContract(CallbackContract = typeof(IParadiseServiceClient))]
	public interface IParadiseServiceHost {
		[OperationContract]
		ParadiseServiceStatus UpdateClientInfo();

		[OperationContract]
		bool StartService(string serviceName, string serviceVersion);

		[OperationContract]
		bool StopService(string serviceName, string serviceVersion);

		[OperationContract]
		bool RestartService(string serviceName, string serviceVersion);

		[OperationContract]
		void StartAllServices();

		[OperationContract]
		void StopAllServices();

		[OperationContract]
		void RestartAllServices();

		[OperationContract]
		bool IsAnyServiceRunning();

		[OperationContract]
		bool IsDatabaseOpen();

		[OperationContract]
		void OpenDatabase();

		[OperationContract]
		void DisposeDatabase();

		[OperationContract]
		bool IsFileServerRunning();

		[OperationContract]
		void StartFileServer();

		[OperationContract]
		void StopFileServer();

		[OperationContract]
		void SendConsoleCommand(string command, string[] arguments);
	}
}
