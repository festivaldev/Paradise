using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Paradise.WebServices {
	public abstract class ParadiseServicePlugin {
		public virtual List<Type> Commands { get; }

		public virtual Dictionary<string, BaseWebService> LoadServices(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			return null;
		}

		public virtual void OnLoad() { }
		public virtual void OnStart() { }
		public virtual void OnStop() { }

		public virtual Dictionary<string, object> HandleTrayQuery() {
			return null;
		}
	}
}
