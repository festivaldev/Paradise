using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices.ServiceHost {
	internal static class Program {
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		static void Main() {
			using (var host = new System.ServiceModel.ServiceHost(typeof(ParadiseService), new Uri("net.pipe://localhost"))) {
				host.AddServiceEndpoint(typeof(IParadiseServiceHost), new NetNamedPipeBinding(), "Paradise.WebServices");
				host.Open();

				ServiceBase.Run(new ParadiseService());

				host.Close();
			}
		}
	}
}
