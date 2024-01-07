using log4net;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

namespace Paradise.WebServices {
	[CallbackBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	public partial class ParadiseControlForm : Form, IParadiseServiceClient {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseControlForm));

		private DuplexChannelFactory<IParadiseServiceHost> ChannelFactory;
		private IParadiseServiceHost ServiceChannel;
		private ParadiseServiceStatus ServiceStatus;

		public ParadiseControlForm() {
			InitializeComponent();
			SetTrayIconEnabled(false);

			ConnectToWebServices(OnPipeOpened, OnPipeError);
		}

		protected override CreateParams CreateParams {
			get {
				var Params = base.CreateParams;
				Params.ExStyle |= 0x80;
				return Params;
			}
		}

		#region Event Handlers
		private void OnTrayIconClicked(object sender, MouseEventArgs args) {
			if (ServiceChannel != null && ((IClientChannel)ServiceChannel).State == CommunicationState.Opened) {
				ServiceStatus = ServiceChannel.UpdateClientInfo();
			}

			if (args.Button == MouseButtons.Left) {
				MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(notifyIcon, null);
			}
		}

		private void QuitMenuItemClicked(object sender, EventArgs args) {
			base.Close();

			Application.Exit();
		}

		private void DisconnectMenuItemClicked(object sender, EventArgs args) {
			DisconnectFromWebServices();
		}

		private void ConnectMenuItemClicked(object sender, EventArgs args) {
			ConnectToWebServices(OnPipeOpened, OnPipeError);
		}

		private void DatabaseOpenMenuItemClicked(object sender, EventArgs args) {
			if (!ServiceChannel.IsDatabaseOpen()) {
				ServiceChannel.OpenDatabase();
				notifyIcon.ShowBalloonTip(3000, "Database connected", "Connected successfully to Paradise database.", ToolTipIcon.None);
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
				process.StartInfo.Arguments = string.Join(Path.DirectorySeparatorChar.ToString(), new string[] { Directory.GetCurrentDirectory(), "logs", "Paradise.WebServices.log" });
				process.Start();
			}
		}

		private void OpenConsoleMenuItemClicked(object sender, EventArgs args) {
			ConsoleHelper.CreateConsole();

			ConsoleHelper.PrintConsoleHeaderSubtitle();

			Thread workerThread = new Thread(new ThreadStart(delegate {
				var RunApp = true;

				while (RunApp) {
					var cmd = ConsolePrompter.Prompt("> ");

					var cmdArgs = cmd.Split(' ').ToList();

					switch (cmdArgs[0].ToLower()) {
						case "clear":
							Console.Clear();

							ConsoleHelper.PrintConsoleHeader();
							ConsoleHelper.PrintConsoleHeaderSubtitle();
							break;
						case "q":
						case "quit":
							RunApp = false;
							break;
						default:
							ServiceChannel.SendConsoleCommand(cmdArgs[0], cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray());
							break;
					}
				}

				ConsoleHelper.DestroyConsole();
			}));

			workerThread.Start();
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

		private void OnStartHttpServerMenuItemClicked(object sender, EventArgs args) {
			if (!ServiceChannel.IsHttpServerRunning()) {
				ServiceChannel.StartHttpServer();
			}
		}

		private void OnStopHttpServerMenuItemClicked(object sender, EventArgs args) {
			if (ServiceChannel.IsHttpServerRunning()) {
				ServiceChannel.StopHttpServer();
			}
		}
		#endregion

		private bool ConnectToWebServices(Action success, Action<Exception> error) {
			connectMenuItem.Enabled = false;

			connectionMenuItem.Text = "Connecting...";

			try {
				ChannelFactory = new DuplexChannelFactory<IParadiseServiceHost>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress($"net.pipe://localhost/{Program.CLIOptions.PipeName ?? "NewParadise.WebServices"}"));
				ServiceChannel = ChannelFactory.CreateChannel();
				((IClientChannel)ServiceChannel).Closed += OnPipeClosed;
				ServiceStatus = ServiceChannel.UpdateClientInfo();
			} catch (Exception e) {
				error?.Invoke(e);

				return false;
			}

			success?.Invoke();

			return true;
		}

		private void DisconnectFromWebServices() {
			((IClientChannel)ServiceChannel).Close();
			ChannelFactory.Close();

			ServiceChannel = null;
			ChannelFactory = null;
		}

		private void OnPipeOpened() {
			connectionMenuItem.Text = "Connected";

			connectMenuItem.Visible = false;
			disconnectMenuItem.Visible = true;

			openConsoleMenuItem.Enabled = true;

			serviceListMenuItem.Enabled = true;

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

			databaseMenuItem.Enabled = true;

			if (ServiceStatus.DatabaseOpened) {
				databaseOpenMenuItem.Enabled = false;
				databaseCloseMenuItem.Enabled = true;
			} else {
				databaseOpenMenuItem.Enabled = true;
				databaseCloseMenuItem.Enabled = false;
			}

			fileServerMenuItem.Enabled = true;

			if (ServiceStatus.FileServerRunning) {
				startHttpServerMenuItem.Enabled = false;
				stopHttpServerMenuItem.Enabled = true;
			} else {
				startHttpServerMenuItem.Enabled = true;
				stopHttpServerMenuItem.Enabled = false;
			}
		}

		private void OnPipeClosed(object sender, EventArgs e) {
			SetTrayIconEnabled(false);

			connectionMenuItem.Text = "Disconnected";

			connectMenuItem.Visible = true;
			connectMenuItem.Enabled = true;

			disconnectMenuItem.Visible = false;

			openConsoleMenuItem.Enabled = false;

			serviceListMenuItem.Enabled = false;

			serviceListMenuItem.DropDownItems[0].Enabled = true;
			serviceListMenuItem.DropDownItems[1].Enabled = false;
			serviceListMenuItem.DropDownItems[2].Enabled = false;

			var serviceItems = serviceListMenuItem.DropDownItems.Cast<ToolStripItem>().ToList();

			foreach (var service in serviceItems) {
				if (service is ToolStripMenuItem && ((ToolStripMenuItem)service).DropDownItems.Count > 0) {
					serviceListMenuItem.DropDownItems.Remove((ToolStripMenuItem)service);
				}
			}

			databaseMenuItem.Enabled = false;

			databaseOpenMenuItem.Enabled = true;
			databaseCloseMenuItem.Enabled = false;

			fileServerMenuItem.Enabled = false;

			startHttpServerMenuItem.Enabled = true;
			stopHttpServerMenuItem.Enabled = false;
		}

		private void OnPipeError(Exception e) {
			MessageBox.Show($"Could not connect to the Paradise Web Services process.\nPlease make sure that the Paradise Web Services are running.\n\n{e.Message}", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Error);

			OnPipeClosed(null, null);
		}

		#region IParadiseServiceClient
		//public void OnError(string message, Exception e) { }

		public void OnError(string title, string message) {
			notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Error);
		}

		public void SetTrayIconEnabled(bool enabled) {
			if (enabled) {
				notifyIcon.Icon = Properties.Resources.RunningIcon;
				notifyIcon.Text = $"Paradise Web Services (Running)";
			} else {
				notifyIcon.Icon = Properties.Resources.StoppedIcon;
				notifyIcon.Text = $"Paradise WebServices (Inactive)";
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

		public void OnDatabaseError(Exception e) {
			notifyIcon.ShowBalloonTip(3000, "Database Error", $"A database error has occurred. Please check the logs.\n{e.Message}", ToolTipIcon.Error);
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

		public void OnHttpServerStarted() {
			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startHttpServerMenuItem.Enabled = false;
				stopHttpServerMenuItem.Enabled = true;
			});
		}

		public void OnHttpServerStopped() {
			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startHttpServerMenuItem.Enabled = true;
				stopHttpServerMenuItem.Enabled = false;
			});
		}

		public void OnHttpServerError(Exception e) {
			notifyIcon.ShowBalloonTip(3000, "HTTP Server Error", e.Message, ToolTipIcon.Error);

			trayMenuStrip.Invoke((MethodInvoker)delegate {
				startHttpServerMenuItem.Enabled = false;
				stopHttpServerMenuItem.Enabled = true;
			});
		}

		public void OnConsoleCommandCallback(string message, bool inline) {
			if (!inline) {
				Console.WriteLine(message);
			} else {
				Console.Write(message);
			}
		}
		#endregion
	}
}
