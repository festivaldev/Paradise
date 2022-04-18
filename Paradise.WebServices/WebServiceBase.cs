using log4net;
using Paradise.Core.Serialization;
using Paradise.Util.Ciphers;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Paradise.WebServices {
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
	public abstract class WebServiceBase {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(WebServiceBase));

		protected ParadiseSettings Settings;

		protected BasicHttpBinding HttpBinding { get; }
		protected EndpointAddress ServiceEndpoint { get; private set; }
		protected ServiceHost ServiceHost { get; private set; }

		public abstract string ServiceName { get; }
		public abstract string ServiceVersion { get; }
		protected abstract Type ServiceInterface { get; }

		protected CryptographyPolicy CryptoPolicy = new CryptographyPolicy();

		protected EventHandler<ServiceEventArgs> ServiceStarted;
		protected EventHandler<ServiceEventArgs> ServiceStopped;
		protected EventHandler<ServiceEventArgs> ServiceError;

		public CommunicationState WebServiceState => ServiceHost?.State ?? CommunicationState.Closed;
		public string State {
			get {
				switch (ServiceHost?.State ?? CommunicationState.Closed) {
					case CommunicationState.Created:
					case CommunicationState.Opening:
					case CommunicationState.Closing:
						return "\u001b[33mWaiting\u001b[0m";
					case CommunicationState.Opened:
						return "\u001b[32mRunning\u001b[0m";
					case CommunicationState.Faulted:
						return "\u001b[31mError\u001b[0m";
					case CommunicationState.Closed:
					default:
						return "\u001b[31mStopped\u001b[0m";
				}
			}
		}

		protected WebServiceBase(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) {
			Log.Info($"Initializing {ServiceName} ({ServiceVersion})...");
			HttpBinding = binding;
			ServiceEndpoint = new EndpointAddress($"{serviceBaseUrl.TrimEnd(new[] { '/' })}/{ServiceVersion}/{webServicePrefix}{ServiceName}{webServiceSuffix}");
		}

		protected WebServiceBase(BasicHttpBinding binding, ParadiseSettings settings, IServiceCallback serviceCallback) {
			Log.Info($"Initializing {ServiceName} ({ServiceVersion})...");

			Settings = settings;
			HttpBinding = binding;

			var uriBuilder = new UriBuilder();
			uriBuilder.Scheme = settings.EnableSSL ? "https" : "http";
			uriBuilder.Host = settings.WebServiceHostName;
			uriBuilder.Port = settings.WebServicePort;
			uriBuilder.Path = $"{ServiceVersion}/{settings.WebServicePrefix}{ServiceName}{settings.WebServiceSuffix}";

			ServiceEndpoint = new EndpointAddress(uriBuilder.ToString());

			ServiceStarted += serviceCallback.OnServiceStarted;
			ServiceStopped += serviceCallback.OnServiceStopped;
			ServiceError += serviceCallback.OnServiceError;
		}

		public bool StartService() {
			if (ServiceHost?.State != CommunicationState.Opening && ServiceHost?.State != CommunicationState.Opened) {
				try {
					ServiceHost = new ServiceHost(this);

					if (Settings.EnableSSL && ServiceEndpoint.Uri.Scheme == "https") {
						ServiceHost.Credentials.ServiceCertificate.SetCertificate(
							StoreLocation.LocalMachine,
							StoreName.My,
							X509FindType.FindBySubjectName,
							ServiceEndpoint.Uri.Host
						);
						ServiceHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
					}

					ServiceHost.AddServiceEndpoint(ServiceInterface, HttpBinding, ServiceEndpoint.Uri);

					ServiceHost.Open();
					Log.Info($"{ServiceName} ({ServiceVersion}) successfully started");
					ServiceStarted?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						HasStarted = true
					});

					Setup();
				} catch (Exception e) {
					Log.Error($"Failed to start service {ServiceName}: {e.Message}");
					ServiceError?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						Starting = true,
						Exception = e
					});
					return false;
				}

				return true;
			}

			return false;
		}

		public bool StopService() {
			if (ServiceHost?.State != CommunicationState.Closing && ServiceHost?.State != CommunicationState.Closed && ServiceHost?.State != CommunicationState.Faulted) {
				try {
					ServiceHost.Close();
					Log.Info($"{ServiceName} ({ServiceVersion}) successfully stopped");
					ServiceStopped?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						HasStopped = true
					});
				} catch (Exception e) {
					Log.Error($"Failed to stop service {ServiceName}: {e.Message}");
					ServiceError?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						Stopping = true,
						Exception = e
					});
					return false;
				}

				return true;
			}

			return false;
		}

		public bool IsRunning => ServiceHost?.State == CommunicationState.Opened;

		protected abstract void Setup();

		protected void DebugEndpoint(params object[] data) {
#if DEBUG
			Log.Debug($"[{DateTime.UtcNow.ToString("o")}] {ServiceName}({ServiceVersion}):{new StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
		}

		protected void HandleEndpointError(Exception e) {
			Log.Error($"Failed to handle {ServiceName}:{e.TargetSite.Name}: {e.Message}{Environment.NewLine}{e.StackTrace}");
			ServiceError?.Invoke(this, new ServiceEventArgs {
				ServiceName = ServiceName,
				ServiceVersion = ServiceVersion,
				Exception = e
			});
		}

		protected SteamMember SteamMemberFromAuthToken(string authToken) {
			using (var inputStream = new MemoryStream(Convert.FromBase64String(authToken))) {
				var steamId = StringProxy.Deserialize(inputStream);
				var validThru = DateTimeProxy.Deserialize(inputStream);

				if (validThru >= DateTime.Now) {
					var steamMember = DatabaseManager.SteamMembers.FindOne(_ => _.SteamId == steamId);
					//Log.Debug($"steam member: {steamMember.SteamId}");
					return steamMember;
				}
			}

			return null;
		}
	}

	//public static class Log {
	//	public static void Debug(string message) {
	//		Console.WriteLine($"[\u001b[35mDEBUG\u001b[0m] {message}");
	//	}
	//	public static void Success(string message) {
	//		Console.WriteLine($"[\u001b[32mOK\u001b[0m] {message}");
	//	}

	//	public static void Info(string message) {
	//		Console.WriteLine($"[\u001b[36mINFO\u001b[0m] {message}");
	//	}

	//	public static void Warn(string message) {
	//		Console.WriteLine($"[\u001b[33mWARN\u001b[0m] {message}");
	//	}

	//	public static void Error(string message) {
	//		Console.WriteLine($"[\u001b[31mERROR\u001b[0m] {message}");
	//	}

	//}

	public class ServiceEventArgs : EventArgs {
		public string ServiceName { get; set; }
		public string ServiceVersion { get; set; }

		public bool Starting { get; set; }
		public bool HasStarted { get; set; }

		public bool Stopping { get; set; }
		public bool HasStopped { get; set; }

		public Exception Exception { get; set; }
	}
}
