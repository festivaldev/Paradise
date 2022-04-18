using log4net;
using log4net.Config;
using Paradise.WebServices.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices.GUI {
	public partial class ParadiseControlForm : Form, IServiceCallback {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ParadiseControlForm));

		public static ParadiseControlForm Instance;

		private static BasicHttpBinding HttpBinding;
		static Dictionary<string, WebServiceBase> Services = new Dictionary<string, WebServiceBase>();
		private static ParadiseSettings WebServiceSettings;

		private SimpleHTTPServer HttpServer;

		public ParadiseControlForm() {
			Instance = this;

			var configFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config"));
			if (configFile.Exists) {
				XmlConfigurator.ConfigureAndWatch(configFile);
			}

			InitializeComponent();
			SetTrayIconEnabled(false);

			XmlSerializer serializer = new XmlSerializer(typeof(ParadiseSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "ParadiseSettings.xml")))) {
					WebServiceSettings = (ParadiseSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				notifyIcon.ShowBalloonTip(3000, "There was an error parsing the settings file.", e.Message, ToolTipIcon.Error);
			}

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			DatabaseManager.OpenDatabase();

			HttpBinding = new BasicHttpBinding();

			if (WebServiceSettings.EnableSSL) {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
				HttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			} else {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.None;
			}

			Services = new Dictionary<string, WebServiceBase> {
				["Application"] = new ApplicationWebService(HttpBinding, WebServiceSettings, this),
				["Authentication"] = new AuthenticationWebService(HttpBinding, WebServiceSettings, this),
				["Clan"] = new ClanWebService(HttpBinding, WebServiceSettings, this),
				["Moderation"] = new ModerationWebService(HttpBinding, WebServiceSettings, this),
				["PrivateMessage"] = new PrivateMessageWebService(HttpBinding, WebServiceSettings, this),
				["Relationship"] = new RelationshipWebService(HttpBinding, WebServiceSettings, this),
				["Shop"] = new ShopWebService(HttpBinding, WebServiceSettings, this),
				["User"] = new UserWebService(HttpBinding, WebServiceSettings, this)
			};

			foreach (var service in Services.Values) {
				var serviceItem = new ToolStripMenuItem {
					Tag = service.ServiceName,
					Text = $"{service.ServiceName} [{service.ServiceVersion}]",
				};

				var startServiceItem = new ToolStripMenuItem {
					Text = $"Start Service",
				};

				startServiceItem.Click += delegate {
					service.StartService();
				};

				serviceItem.DropDownItems.Add(startServiceItem);

				var stopServiceItem = new ToolStripMenuItem {
					Text = $"Stop Service",
				};

				stopServiceItem.Click += delegate {
					service.StopService();
				};

				serviceItem.DropDownItems.Add(stopServiceItem);

				var restartServiceItem = new ToolStripMenuItem {
					Text = $"Restart Service",
				};

				restartServiceItem.Click += delegate {
					Log.Warn($"Restarting {service.ServiceName} [{service.ServiceVersion}]...");

					service.StopService();

					if (!service.StartService()) {
						notifyIcon.ShowBalloonTip(3000, "Service Restart Failed", $"Service {service.ServiceName} [{service.ServiceVersion}] failed to restart. Please check the log file for further information.", ToolTipIcon.Error);
					}
				};

				serviceItem.DropDownItems.Add(restartServiceItem);

				serviceListMenuItem.DropDownItems.Add(serviceItem);

				service.StartService();
			}

			HttpServer = new SimpleHTTPServer(Path.Combine(Directory.GetCurrentDirectory(), "www"), WebServiceSettings);

			try {
				HttpServer.Start();

				startFileServerMenuItem.Enabled = false;
				stopFileServerMenuItem.Enabled = true;
			} catch (Exception e) {
				Log.Error(e);
				notifyIcon.ShowBalloonTip(3000, "File Server failed to start", e.Message, ToolTipIcon.Error);

				startFileServerMenuItem.Enabled = true;
				stopFileServerMenuItem.Enabled = false;
			}
		}

		private void SetTrayIconEnabled(bool enabled) {
			if (enabled) {
				notifyIcon.Icon = Properties.Resources.RunningIcon;
				notifyIcon.Text = $"{Properties.Resources.ApplicationDisplayName} (Running)";
			} else {
				notifyIcon.Icon = Properties.Resources.StoppedIcon;
				notifyIcon.Text = $"{Properties.Resources.ApplicationDisplayName} (Inactive)";
			}
		}

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			databaseOpenMenuItem.Enabled = false;
			databaseCloseMenuItem.Enabled = true;
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			databaseOpenMenuItem.Enabled = true;
			databaseCloseMenuItem.Enabled = false;
		}

		//private void OnDatabaseError(object sender, ErrorEventArgs args) {
		//	Log.Error(args.GetException());
		//	notifyIcon.ShowBalloonTip(3000, "Database error", args.GetException().Message, ToolTipIcon.Error);
		//	notifyIcon.BalloonTipClicked += OpenLogMenuItemClicked;
		//}
		#endregion

		#region Service Callbacks
		public void OnServiceStarted(object sender, ServiceEventArgs args) {
			if (Services.Values.Any(_ => _.IsRunning)) {
				SetTrayIconEnabled(true);

				serviceListMenuItem.DropDownItems[0].Enabled = false;
				serviceListMenuItem.DropDownItems[1].Enabled = true;
			} else {
				SetTrayIconEnabled(false);

				serviceListMenuItem.DropDownItems[0].Enabled = true;
				serviceListMenuItem.DropDownItems[1].Enabled = false;
			}

			var serviceMenuItem = serviceListMenuItem.DropDownItems.Cast<ToolStripItem>().Where(_ => _.Text.StartsWith(args.ServiceName)).FirstOrDefault();
			if (serviceMenuItem != null) {
				((ToolStripMenuItem)serviceMenuItem).DropDownItems[0].Enabled = false;
				((ToolStripMenuItem)serviceMenuItem).DropDownItems[1].Enabled = true;
			}
		}

		public void OnServiceStopped(object sender, ServiceEventArgs args) {
			if (Services.Values.Any(_ => _.IsRunning)) {
				SetTrayIconEnabled(true);

				serviceListMenuItem.DropDownItems[0].Enabled = false;
				serviceListMenuItem.DropDownItems[1].Enabled = true;
			} else {
				SetTrayIconEnabled(false);

				serviceListMenuItem.DropDownItems[0].Enabled = true;
				serviceListMenuItem.DropDownItems[1].Enabled = false;
			}

			var serviceMenuItem = serviceListMenuItem.DropDownItems.Cast<ToolStripItem>().Where(_ => _.Text.StartsWith(args.ServiceName)).FirstOrDefault();
			if (serviceMenuItem != null) {
				((ToolStripMenuItem)serviceMenuItem).DropDownItems[0].Enabled = true;
				((ToolStripMenuItem)serviceMenuItem).DropDownItems[1].Enabled = false;
			}
		}

		public void OnServiceError(object sender, ServiceEventArgs args) {
			if (args.Starting) {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error while starting {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			} else if (args.Stopping) {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error while stopping {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			} else {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error in {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			}

			notifyIcon.BalloonTipClicked += OpenLogMenuItemClicked;
		}
		#endregion

		#region Event Handlers
		private void OnTrayIconClicked(object sender, MouseEventArgs args) {
			if (args.Button == MouseButtons.Left) {
				MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(notifyIcon, null);
			}
		}

		private void QuitMenuItemClicked(object sender, EventArgs args) {
			HttpServer.Stop();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			DatabaseManager.DisposeDatabase();

			base.Close();
			Task.Run(async () => {
				await Task.Delay(300);

				Application.Exit();
			});
		}

		private void DatabaseOpenMenuItemClicked(object sender, EventArgs args) {
			if (!DatabaseManager.IsOpen) {
				DatabaseManager.OpenDatabase();
				notifyIcon.ShowBalloonTip(3000, "Database connected", "Connectied successfully to Paradise database.", ToolTipIcon.None);
			}
		}

		private void DatabaseCloseMenuItemClicked(object sender, EventArgs args) {
			if (DatabaseManager.IsOpen) {
				DatabaseManager.DisposeDatabase();
				notifyIcon.ShowBalloonTip(3000, "Database disconnected", "Disconnected from Paradise database.", ToolTipIcon.None);
			}
		}

		private void OpenLogMenuItemClicked(object sender, EventArgs args) {
			notifyIcon.Click -= OpenLogMenuItemClicked;

			using (Process process = new Process()) {
				process.StartInfo.FileName = "explorer.exe";
				process.StartInfo.Arguments = Path.Combine(Directory.GetCurrentDirectory(), "Paradise.WebServices.log");
				process.Start();
			}
		}

		private void OnStartAllServicesMenuItemClicked(object sender, EventArgs args) {
			foreach (var service in Services.Values) {
				service.StartService();
			}
		}

		private void OnStopAllServicesMenuItemClicked(object sender, EventArgs args) {
			foreach (var service in Services.Values) {
				service.StopService();
			}
		}

		private void OnRestartAllServicesMenuItemClicked(object sender, EventArgs args) {
			Log.Warn($"Restarting all services...");
			foreach (var service in Services.Values) {
				service.StopService();

				if (!service.StartService()) {
					notifyIcon.ShowBalloonTip(3000, "Service Restart Failed", $"Service {service.ServiceName} [{service.ServiceVersion}] failed to restart. Please check the log file for further information.", ToolTipIcon.Error);
				}
			}
		}

		private void OnStartFileServerMenuItemClicked(object sender, EventArgs args) {
			if (!HttpServer.IsRunning) {
				try {
					HttpServer.Start();

					startFileServerMenuItem.Enabled = false;
					stopFileServerMenuItem.Enabled = true;
				} catch (Exception e) {
					Log.Error(e);
					notifyIcon.ShowBalloonTip(3000, "File Server failed to start", e.Message, ToolTipIcon.Error);

					startFileServerMenuItem.Enabled = true;
					stopFileServerMenuItem.Enabled = false;
				}
			}
		}

		private void OnStopFileServerMenuItemClicked(object sender, EventArgs args) {
			if (HttpServer.IsRunning) {
				try {
					HttpServer.Stop();

					startFileServerMenuItem.Enabled = true;
					stopFileServerMenuItem.Enabled = false;
				} catch (Exception e) {
					Log.Error(e);
					notifyIcon.ShowBalloonTip(3000, "File Server failed to stop", e.Message, ToolTipIcon.Error);

					startFileServerMenuItem.Enabled = false;
					stopFileServerMenuItem.Enabled = true;
				}
			}
		}
		#endregion
	}
}
