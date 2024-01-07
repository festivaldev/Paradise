using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices {
	public enum PluginQueryType {
		IsDatabaseOpen,
		OpenDatabase,
		DisposeDatabase
	}

	public abstract class ParadiseServicePlugin {
		public static string ServiceDataPath => Path.Combine(ParadiseService.WorkingDirectory, "ServiceData");

		public virtual List<Type> Commands { get; }

		public virtual Dictionary<string, BaseWebService> LoadServices(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) {
			return null;
		}

		public virtual void OnLoad() { }
		public virtual void OnStart() { }
		public virtual void OnStop() { }
	}
}
