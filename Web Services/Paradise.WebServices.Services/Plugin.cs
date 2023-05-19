using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class Plugin : ParadiseServicePlugin {
		public override void OnStart() {
			base.OnStart();

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
		}

		public override void OnStop() {
			base.OnStop();

			DatabaseManager.DatabaseOpened -= OnDatabaseOpened;
			DatabaseManager.DatabaseClosed -= OnDatabaseClosed;
		}

		public override List<Type> Commands => new List<Type> {
			typeof(BanCommand),
			typeof(DeopCommand),
			typeof(InventoryCommand),
			typeof(OpCommand),
			typeof(PlayersCommand),
			typeof(RoomsCommand),
			typeof(UnbanCommand),
			typeof(WalletCommand),
			typeof(XpCommand)
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
			DatabaseClient.LoadCollections();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			DatabaseClient.UnloadCollections();
		}

		private void OnDatabaseError(object sender, ErrorEventArgs args) {
			ParadiseService.Instance.ClientCallback?.OnDatabaseError(args.GetException());
		}
		#endregion
	}
}
