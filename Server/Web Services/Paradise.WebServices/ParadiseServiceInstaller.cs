using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Paradise.WebServices {
	[RunInstaller(true)]
	public partial class ParadiseServiceInstaller : Installer {
		private readonly ServiceProcessInstaller ServiceProcessInstaller;
		private readonly ServiceInstaller ServiceInstaller;

		public ParadiseServiceInstaller() {
			ServiceProcessInstaller = new ServiceProcessInstaller {
				Account = ServiceAccount.LocalSystem
			};

			ServiceInstaller = new ServiceInstaller {
				ServiceName = Program.CLIOptions.ServiceName ?? "NewParadise.WebServices",
				DisplayName = "Paradise Web Services",
				Description = "Service for running the Web Services API and File Server used by Paradise, a UberStrike Server implementation.",
				StartType = ServiceStartMode.Automatic
			};

			this.Installers.AddRange(new Installer[] {
				ServiceProcessInstaller,
				ServiceInstaller
			});

			this.BeforeInstall += new InstallEventHandler(BeforeInstallHandler);
			this.AfterInstall += new InstallEventHandler(AfterInstallHandler);

			this.BeforeUninstall += new InstallEventHandler(BeforeUninstallHandler);
			this.AfterUninstall += new InstallEventHandler(AfterUninstallHandler);
		}

		private void BeforeInstallHandler(object sender, InstallEventArgs e) {
			if (!Context.Parameters["assemblypath"].EndsWith("--svc")) {
				Context.Parameters["assemblypath"] = $"\"{Context.Parameters["assemblypath"]}\" --svc";
			}

			Context.Parameters["assemblypath"] += $" --service-name {Program.CLIOptions.ServiceName}";
			Context.Parameters["assemblypath"] += $" --pipe-name {Program.CLIOptions.PipeName}";
		}

		private void AfterInstallHandler(object sender, InstallEventArgs e) {
			using (ServiceController serviceController = new ServiceController(((ParadiseServiceInstaller)sender).ServiceInstaller.ServiceName)) {
				serviceController.Start();
			}
		}

		private void BeforeUninstallHandler(object sender, InstallEventArgs e) {
			using (ServiceController serviceController = new ServiceController(((ParadiseServiceInstaller)sender).ServiceInstaller.ServiceName)) {
				if (serviceController.Status == ServiceControllerStatus.Running) {
					serviceController.Stop();
				}
			}
		}

		private void AfterUninstallHandler(object sender, InstallEventArgs e) {
			if (File.Exists(Path.Combine(Assembly.GetExecutingAssembly().Location, "Paradise.WebServices.InstallState"))) {
				File.Delete(Path.Combine(Assembly.GetExecutingAssembly().Location, "Paradise.WebServices.InstallState"));
			}
		}
	}
}
