using Paradise.Core.Serialization;
using Paradise.Util.Ciphers;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;

namespace Paradise.WebServices {
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
	public abstract class WebServiceBase {
		protected BasicHttpBinding HttpBinding { get; }
		protected EndpointAddress ServiceEndpoint { get; private set; }
		protected ServiceHost ServiceHost { get; private set; }

		protected abstract string ServiceName { get; }
		public abstract string ServiceVersion { get; }
		protected abstract Type ServiceInterface { get; }

		protected CryptographyPolicy CryptoPolicy = new CryptographyPolicy();

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
			//Console.WriteLine(ServiceEndpoint.Uri);

			StartService();
			Setup();
		}

		public bool StartService() {
			if (ServiceHost?.State != CommunicationState.Opening && ServiceHost?.State != CommunicationState.Opened) {
				try {
					ServiceHost = new ServiceHost(this);
					ServiceHost.AddServiceEndpoint(ServiceInterface, HttpBinding, ServiceEndpoint.Uri);

					ServiceHost.Open();
					Log.Success($"{ServiceName} ({ServiceVersion}) successfully started");
				} catch (Exception e) {
					Log.Error($"Failed to start service {ServiceName}: {e.Message}");
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
					Log.Success($"{ServiceName} ({ServiceVersion}) successfully stopped");
				} catch (Exception e) {
					Log.Error($"Failed to stop service {ServiceName}: {e.Message}");
					return false;
				}

				return true;
			}

			return false;
		}

		protected abstract void Setup();

		protected void DebugEndpoint(params object[] data) {
		#if DEBUG
			Log.Debug($"[{DateTime.UtcNow.ToString("o")}] {ServiceName}({ServiceVersion}):{new StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
		#endif
		}

		protected void HandleEndpointError(Exception e) {
			Log.Error($"Failed to handle {ServiceName}:{e.TargetSite.Name}: {e.Message}{Environment.NewLine}{e.StackTrace}");
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

	public static class Log {
		public static void Debug(string message) {
			Console.WriteLine($"[\u001b[35mDEBUG\u001b[0m] {message}");
		}
		public static void Success(string message) {
			Console.WriteLine($"[\u001b[32mOK\u001b[0m] {message}");
		}

		public static void Info(string message) {
			Console.WriteLine($"[\u001b[36mINFO\u001b[0m] {message}");
		}

		public static void Warn(string message) {
			Console.WriteLine($"[\u001b[33mWARN\u001b[0m] {message}");
		}

		public static void Error(string message) {
			Console.WriteLine($"[\u001b[31mERROR\u001b[0m] {message}");
		}

	}
}
