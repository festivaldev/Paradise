using UnityEngine;

namespace Paradise.Client {
	internal class PreloadOptionsPanelButton : MonoBehaviour {
		public void OnGUI() {
			GUI.depth = -150;

			if (GUI.Button(new Rect(Screen.width - 32f, 0, 32f, 32f), new GUIContent(GlobalUiIcons.QuadpanelButtonOptions), BlueStonez.buttondark_medium)) {
				PanelManager.Instance.OpenPanel(PanelType.Options);
			}
		}
	}
}
