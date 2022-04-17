using UnityEngine;

namespace Paradise.Client {
	internal class ParadiseApplicationManager : MonoBehaviour {
		private void OnApplicationQuit() {
			System.Diagnostics.Process.GetCurrentProcess().Close();
		}
	}
}
