using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace Paradise.Client.DiscordRPC {
	internal class Program {
		static void Main(string[] args) {
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
				var assemblyName = new AssemblyName(e.Name).Name;
				var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(_ => _.EndsWith($"{assemblyName}.dll"));

				if (string.IsNullOrEmpty(resourceName)) return null;

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
					return Assembly.Load(new BinaryReader(stream).ReadBytes((int)stream.Length));
				}
			};

			try {
				RichPresenceManager.Initialize();

				using (ServiceHost host = new ServiceHost(typeof(RichPresenceManager))) {

					var binding = new NetTcpBinding();
					binding.Security.Mode = SecurityMode.None;

					host.AddServiceEndpoint(typeof(RpcServiceHost), binding, "net.tcp://localhost/NewParadise.Client.DiscordRPC");

					host.Open();

					Console.WriteLine("Service is available. " +
					  "Press <ENTER> to exit.");

					Console.ReadLine();

					host.Close();
				}
			} catch (Exception e) {
				Console.WriteLine(e);
				Console.ReadLine();
			}
		}
	}
}
