using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class Plugin : ParadiseServicePlugin {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseServicePlugin));

		public override void OnStart() {
			base.OnStart();

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			DatabaseManager.OpenDatabase();
		}

		public override void OnStop() {
			base.OnStop();

			DatabaseManager.DatabaseOpened -= OnDatabaseOpened;
			DatabaseManager.DatabaseClosed -= OnDatabaseClosed;
			DatabaseManager.DisposeDatabase();
		}

		public override List<IParadiseCommand> Commands => new List<IParadiseCommand> {
			new BanCommand(),
			new CreditsCommand(),
			new DatabaseCommand(),
			new DeopCommand(),
			new InventoryCommand(),
			new OpCommand(),
			new PointsCommand(),
			new UnbanCommand(),
			new XpCommand()
		};

		public override Dictionary<string, BaseWebService> LoadServices(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			return new Dictionary<string, BaseWebService> {
				["Application"] = new ApplicationWebService(binding, settings, serviceCallback),
				["Authentication"] = new AuthenticationWebService(binding, settings, serviceCallback),
				["Clan"] = new ClanWebService(binding, settings, serviceCallback),
				["Moderation"] = new ModerationWebService(binding, settings, serviceCallback),
				["PrivateMessage"] = new PrivateMessageWebService(binding, settings, serviceCallback),
				["Relationship"] = new RelationshipWebService(binding, settings, serviceCallback),
				["Shop"] = new ShopWebService(binding, settings, serviceCallback),
				["User"] = new UserWebService(binding, settings, serviceCallback)
			};
		}

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			ParadiseService.Instance.ClientCallback?.OnDatabaseOpened();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			ParadiseService.Instance.ClientCallback?.OnDatabaseClosed();
		}

		//private void OnDatabaseError(object sender, ErrorEventArgs args) {
		//	Log.Error(args.GetException());
		//	notifyIcon.ShowBalloonTip(3000, "Database error", args.GetException().Message, ToolTipIcon.Error);
		//	notifyIcon.BalloonTipClicked += OpenLogMenuItemClicked;
		//}
		#endregion
	}
}
