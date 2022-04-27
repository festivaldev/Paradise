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
	[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	public partial class ParadiseControlForm : Form, IParadiseServiceClient {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ParadiseControlForm));

		private IParadiseServiceHost ServiceChannel;
		private ParadiseServiceStatus ServiceStatus;

		public ParadiseControlForm() {
			var configFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config"));
			if (configFile.Exists) {
				XmlConfigurator.ConfigureAndWatch(configFile);
			}

			var factory = new DuplexChannelFactory<IParadiseServiceHost>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Paradise.WebServices"));
			ServiceChannel = factory.CreateChannel();
			ServiceStatus = ServiceChannel.UpdateClientInfo();

			InitializeComponent();

			if (ServiceChannel.IsAnyServiceRunning()) {
				SetTrayIconEnabled(true);

				serviceListMenuItem.DropDownItems[0].Enabled = false;
				serviceListMenuItem.DropDownItems[1].Enabled = true;
				serviceListMenuItem.DropDownItems[2].Enabled = true;
			} else {
				SetTrayIconEnabled(false);

				serviceListMenuItem.DropDownItems[0].Enabled = true;
				serviceListMenuItem.DropDownItems[1].Enabled = false;
				serviceListMenuItem.DropDownItems[2].Enabled = false;
			}

			foreach (var service in ServiceStatus.Services) {
				var serviceItem = new ToolStripMenuItem {
					Tag = service["ServiceName"],
					Text = $"{service["ServiceName"]} [{service["ServiceVersion"]}]",
				};

				var startServiceItem = new ToolStripMenuItem {
					Text = $"Start Service",
					Enabled = !(bool)service["IsRunning"]
				};

				startServiceItem.Click += delegate {
					ServiceChannel.StartService((string)service["ServiceName"], (string)service["ServiceVersion"]);
				};

				serviceItem.DropDownItems.Add(startServiceItem);

				var stopServiceItem = new ToolStripMenuItem {
					Text = $"Stop Service",
					Enabled = (bool)service["IsRunning"]
				};

				stopServiceItem.Click += delegate {
					ServiceChannel.StopService((string)service["ServiceName"], (string)service["ServiceVersion"]);
				};

				serviceItem.DropDownItems.Add(stopServiceItem);

				var restartServiceItem = new ToolStripMenuItem {
					Text = $"Restart Service",
					Enabled = (bool)service["IsRunning"]
				};

				restartServiceItem.Click += delegate {
					Log.Warn($"Restarting {service["ServiceName"]} [{service["ServiceVersion"]}]...");

					if (!ServiceChannel.RestartService((string)service["ServiceName"], (string)service["ServiceVersion"])) {
						notifyIcon.ShowBalloonTip(3000, "Service Restart Failed", $"Service {service["ServiceName"]} [{service["ServiceVersion"]}] failed to restart. Please check the log file for further information.", ToolTipIcon.Error);
					}
				};

				serviceItem.DropDownItems.Add(restartServiceItem);

				serviceListMenuItem.DropDownItems.Add(serviceItem);
			}

			if (ServiceStatus.DatabaseOpened) {
				databaseOpenMenuItem.Enabled = false;
				databaseCloseMenuItem.Enabled = true;
			} else {
				databaseOpenMenuItem.Enabled = true;
				databaseCloseMenuItem.Enabled = false;
			}

			if (ServiceStatus.FileServerRunning) {
				startFileServerMenuItem.Enabled = false;
				stopFileServerMenuItem.Enabled = true;
			} else {
				startFileServerMenuItem.Enabled = true;
				stopFileServerMenuItem.Enabled = false;
			}
		}

		#region Event Handlers
		private void OnTrayIconClicked(object sender, MouseEventArgs args) {
			if (args.Button == MouseButtons.Left) {
				MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(notifyIcon, null);
			}
		}

		private void QuitMenuItemClicked(object sender, EventArgs args) {
			base.Close();

			Application.Exit();
		}

		private void DatabaseOpenMenuItemClicked(object sender, EventArgs args) {
			if (!ServiceChannel.IsDatabaseOpen()) {
				ServiceChannel.OpenDatabase();
				notifyIcon.ShowBalloonTip(3000, "Database connected", "Connectied successfully to Paradise database.", ToolTipIcon.None);
			}
		}

		private void DatabaseCloseMenuItemClicked(object sender, EventArgs args) {
			if (ServiceChannel.IsDatabaseOpen()) {
				ServiceChannel.DisposeDatabase();
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
			ServiceChannel.StartAllServices();
		}

		private void OnStopAllServicesMenuItemClicked(object sender, EventArgs args) {
			ServiceChannel.StopAllServices();
		}

		private void OnRestartAllServicesMenuItemClicked(object sender, EventArgs args) {
			Log.Warn($"Restarting all services...");
			ServiceChannel.RestartAllServices();
		}

		private void OnStartFileServerMenuItemClicked(object sender, EventArgs args) {
			if (!ServiceChannel.IsFileServerRunning()) {
				ServiceChannel.StartFileServer();
			}
		}

		private void OnStopFileServerMenuItemClicked(object sender, EventArgs args) {
			if (ServiceChannel.IsFileServerRunning()) {
				ServiceChannel.StopFileServer();
			}
		}
		#endregion

		#region IParadiseServiceClient
		//public void OnError(string message, Exception e) { }

		public void OnError(string title, string message) {
			notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Error);
		}

		public void SetTrayIconEnabled(bool enabled) {
			if (enabled) {
				notifyIcon.Icon = Properties.Resources.RunningIcon;
				notifyIcon.Text = $"{Properties.Resources.ApplicationDisplayName} (Running)";
			} else {
				notifyIcon.Icon = Properties.Resources.StoppedIcon;
				notifyIcon.Text = $"{Properties.Resources.ApplicationDisplayName} (Inactive)";
			}
		}

		public void OnDatabaseOpened() {
			notifyIcon.ShowBalloonTip(3000, "Database connected", "Connectied successfully to Paradise database.", ToolTipIcon.None);

			trayMenuStrip.Invoke((MethodInvoker)delegate {
				databaseOpenMenuItem.Enabled = false;
				databaseCloseMenuItem.Enabled = true;
			});
		}

		public void OnDatabaseClosed() {
			notifyIcon.ShowBalloonTip(3000, "Database disconnected", "Disconnected from Paradise database.", ToolTipIcon.None);

			trayMenuStrip.Invoke((MethodInvoker)delegate {
				databaseOpenMenuItem.Enabled = true;
				databaseCloseMenuItem.Enabled = false;
			});
		}

		public void OnDatabaseError() {

		}

		public void OnServiceStarted(ServiceEventArgs args) {
			if (ServiceChannel.IsAnyServiceRunning()) {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					SetTrayIconEnabled(true);

					serviceListMenuItem.DropDownItems[0].Enabled = false;
					serviceListMenuItem.DropDownItems[1].Enabled = true;
					serviceListMenuItem.DropDownItems[2].Enabled = true;
				});
			} else {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					SetTrayIconEnabled(false);

					serviceListMenuItem.DropDownItems[0].Enabled = true;
					serviceListMenuItem.DropDownItems[1].Enabled = false;
					serviceListMenuItem.DropDownItems[2].Enabled = false;
				});
			}

			var serviceMenuItem = serviceListMenuItem.DropDownItems.Cast<ToolStripItem>().Where(_ => _.Text.StartsWith(args.ServiceName)).FirstOrDefault();
			if (serviceMenuItem != null) {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[0].Enabled = false;
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[1].Enabled = true;
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[2].Enabled = true;
				});
			}
		}

		public void OnServiceStopped(ServiceEventArgs args) {
			if (ServiceChannel.IsAnyServiceRunning()) {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					SetTrayIconEnabled(true);

					serviceListMenuItem.DropDownItems[0].Enabled = false;
					serviceListMenuItem.DropDownItems[1].Enabled = true;
					serviceListMenuItem.DropDownItems[2].Enabled = true;
				});
			} else {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					SetTrayIconEnabled(false);

					serviceListMenuItem.DropDownItems[0].Enabled = true;
					serviceListMenuItem.DropDownItems[1].Enabled = false;
					serviceListMenuItem.DropDownItems[2].Enabled = false;
				});
			}

			var serviceMenuItem = serviceListMenuItem.DropDownItems.Cast<ToolStripItem>().Where(_ => _.Text.StartsWith(args.ServiceName)).FirstOrDefault();
			if (serviceMenuItem != null) {
				trayMenuStrip.Invoke((MethodInvoker)delegate {
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[0].Enabled = true;
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[1].Enabled = false;
					((ToolStripMenuItem)serviceMenuItem).DropDownItems[2].Enabled = false;
				});
			}
		}

		public void OnServiceError(ServiceEventArgs args) {
			if (args.Starting) {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error while starting {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			} else if (args.Stopping) {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error while stopping {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			} else {
				notifyIcon.ShowBalloonTip(3000, "Web Service Error", $"Error in {args.ServiceName} ({args.ServiceVersion}):\n{args.Exception.Message}", ToolTipIcon.Error);
			}

			notifyIcon.BalloonTipClicked += OpenLogMenuItemClicked;
		}

		public void OnFileServerStarted() {
			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startFileServerMenuItem.Enabled = false;
				stopFileServerMenuItem.Enabled = true;
			});
		}

		public void OnFileServerStopped() {
			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startFileServerMenuItem.Enabled = true;
				stopFileServerMenuItem.Enabled = false;
			});
		}

		public void OnFileServerError(Exception e) {
			notifyIcon.ShowBalloonTip(3000, "File Server Error", e.Message, ToolTipIcon.Error);

			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startFileServerMenuItem.Enabled = false;
				stopFileServerMenuItem.Enabled = true;
			});
		}
		#endregion
	}
}
