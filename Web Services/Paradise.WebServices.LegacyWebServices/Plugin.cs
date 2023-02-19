using System.Collections.Generic;
using System.ServiceModel;

namespace Paradise.WebServices.LegacyWebServices {
	public class Plugin : ParadiseServicePlugin {
		public override Dictionary<string, BaseWebService> LoadServices(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			return new Dictionary<string, BaseWebService> {
				["Application_102"] = new ApplicationWebService(binding, settings, serviceCallback),
				["Authentication_102"] = new AuthenticationWebService(binding, settings, serviceCallback),
				["Clan_102"] = new ClanWebService(binding, settings, serviceCallback),
				["Moderation_102"] = new ModerationWebService(binding, settings, serviceCallback),
				["PrivateMessage_102"] = new PrivateMessageWebService(binding, settings, serviceCallback),
				["Relationship_102"] = new RelationshipWebService(binding, settings, serviceCallback),
				["Shop_102"] = new ShopWebService(binding, settings, serviceCallback),
				["User_102"] = new UserWebService(binding, settings, serviceCallback),
			};
		}
	}
}
