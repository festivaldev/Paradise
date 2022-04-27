using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Paradise.WebServices.GUI {
	internal static class Program {
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main() {
			using (var mutex = new Mutex(false, "tf.festival.Paradise.WebServices.GUI")) {
				if (!mutex.WaitOne(TimeSpan.Zero)) return;

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				new ParadiseControlForm();

				Application.Run();

				mutex.ReleaseMutex();
			}
		}
	}
}
