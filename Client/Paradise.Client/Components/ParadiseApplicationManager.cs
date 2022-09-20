using log4net;
using UnityEngine;

namespace Paradise.Client {
	internal class ParadiseApplicationManager : MonoBehaviour {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseApplicationManager));

		private void OnApplicationQuit() {
			Log.Info("Forcing UberStrike to quit");

			System.Diagnostics.Process.GetCurrentProcess().Close();
		}
	}
}
