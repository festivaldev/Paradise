using log4net;
using Paradise.Util.Ciphers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;

namespace Paradise.WebServices {
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
	public abstract class BaseWebService {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseWebService));

		protected ParadiseServerSettings Settings;

		protected BasicHttpBinding HttpBinding { get; }
		protected EndpointAddress ServiceEndpoint { get; private set; }
		protected ServiceHost ServiceHost { get; private set; }

		public abstract string ServiceName { get; }
		public abstract string ServiceVersion { get; }
		protected abstract Type ServiceInterface { get; }

		protected EventHandler<ServiceEventArgs> ServiceStarted;
		protected EventHandler<ServiceEventArgs> ServiceStopped;
		protected EventHandler<ServiceEventArgs> ServiceError;

		public string ServiceDataPath => Path.Combine(ParadiseService.WorkingDirectory, "ServiceData", ServiceName, ServiceVersion);

		public CommunicationState WebServiceState => ServiceHost?.State ?? CommunicationState.Closed;
		public bool IsRunning => ServiceHost?.State == CommunicationState.Opened;

		protected readonly ICryptographyPolicy CryptoPolicy = new CryptographyPolicy();
		protected string EncryptionPassPhrase => ParadiseService.WebServiceSettings.EncryptionPassPhrase;
		protected string EncryptionInitVector => ParadiseService.WebServiceSettings.EncryptionInitVector;

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

		protected BaseWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			Log.Debug($"Initializing {ServiceName} ({ServiceVersion})...");

			Settings = settings;
			HttpBinding = binding;

			var uriBuilder = new UriBuilder {
				Scheme = Settings.EnableSSL ? "https" : "http",
				Host = string.IsNullOrEmpty(Settings.Hostname) ? "localhost" : Settings.Hostname,
				Port = Settings.WebServicePort,
				Path = $"{ServiceVersion}/{Settings.WebServicePrefix}{ServiceName}{Settings.WebServiceSuffix}"
			};

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
						var certificate = !string.IsNullOrWhiteSpace(Settings.SSLCertificateName) ? Settings.SSLCertificateName : ServiceEndpoint.Uri.Host;

						Log.Debug($"Using HTTPS with certificate \"{certificate}\".");

						ServiceHost.Credentials.ServiceCertificate.SetCertificate(
							StoreLocation.LocalMachine,
							StoreName.My,
							X509FindType.FindBySubjectName,
							certificate
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
					Log.Debug(e);

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
					Teardown();

					ServiceHost.Close();

					Log.Info($"{ServiceName} ({ServiceVersion}) successfully stopped");

					ServiceStopped?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						HasStopped = true
					});
				} catch (Exception e) {
					Log.Error($"Failed to stop service {ServiceName}: {e.Message}");
					Log.Debug(e);

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

		protected abstract void Setup();
		protected abstract void Teardown();

		protected void DebugEndpoint(MethodBase serviceMethod, params object[] data) {
			Log.Debug($"[{DateTime.UtcNow:o}] {ServiceName}({ServiceVersion}):{serviceMethod.Name} {{\r\n\t{string.Join("\r\n\t", data.Select(_ => $"[{_.GetType()}] {_}"))}\r\n}}");
		}

		protected void HandleEndpointError(Exception e) {
			Log.Error($"Failed to handle {ServiceName}:{e.TargetSite.Name}: {e.Message}");
			Log.Info(e);

			ServiceError?.Invoke(this, new ServiceEventArgs {
				ServiceName = ServiceName,
				ServiceVersion = ServiceVersion,
				Exception = e
			});
		}



		protected bool IsEncrypted(byte[] data) {
			try {
				CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector);
				return true;
			} catch (CryptographicException) {
				return false;
			}
		}
	}
}
