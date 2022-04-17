using System;
using System.IO;
using System.Windows.Forms;

namespace Paradise.WebServices.GUI {
	internal static class Program {
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			new ParadiseControlForm();

			Application.Run();
		}
	}
}
