using CommandLine;

namespace Paradise.WebServices {
	public class CLIOptions {
		[Option("install", HelpText = "Installs the Paradise Web Services as a Windows Service")]
		public bool InstallService { get; private set; }

		[Option("uninstall", HelpText = "Removes the Paradise Web Services from Windows Services")]
		public bool UninstallService { get; private set; }

		[Option("silent", HelpText = "Disables feedback when (un-)installing Paradise as a Windows Service.")]
		public bool Silent { get; private set; }

		[Option('c', "console", HelpText = "Runs Paradise.WebServices in Console mode.")]
		public bool ConsoleMode { get; private set; }

		[Option("service", HelpText = "Runs Paradise.WebServices in Windows Service mode. (Alias: --svc)")]
		public bool ServiceMode { get; set; }

		[Option("svc", Hidden = true)]
		public bool _ServiceMode {
			get { return ServiceMode; }
			private set { ServiceMode = value; }
		}

		[Option("tray", HelpText = "Runs Paradise.WebServices in Tray mode.")]
		public bool TrayMode { get; private set; }

		[Option("gui", Hidden = true)]
		public bool GUIMode { get; private set; }

		[Option("pipe-name", Hidden = true, Default = "NewParadise.WebServices")]
		public string PipeName { get; private set; }

		[Option("service-name", Hidden = true, Default = "NewParadise.WebServices")]
		public string ServiceName { get; private set; }
	}
}
