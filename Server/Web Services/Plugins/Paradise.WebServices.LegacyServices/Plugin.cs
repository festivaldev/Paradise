using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices {
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

		public override Dictionary<string, BaseWebService> LoadServices(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			return new Dictionary<string, BaseWebService> {
				["Application_102"] = new _102.ApplicationWebService(binding, settings, serviceCallback),
				["Authentication_102"] = new _102.AuthenticationWebService(binding, settings, serviceCallback),
				["Clan_102"] = new _102.ClanWebService(binding, settings, serviceCallback),
				["Moderation_102"] = new _102.ModerationWebService(binding, settings, serviceCallback),
				["PrivateMessage_102"] = new _102.PrivateMessageWebService(binding, settings, serviceCallback),
				["Relationship_102"] = new _102.RelationshipWebService(binding, settings, serviceCallback),
				["Shop_102"] = new _102.ShopWebService(binding, settings, serviceCallback),
				["User_102"] = new _102.UserWebService(binding, settings, serviceCallback)
			};
		}

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			DatabaseClient.LoadCollections();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			DatabaseClient.UnloadCollections();
		}
		#endregion
	}
}
