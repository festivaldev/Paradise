using HarmonyLib;
using log4net;
using UnityEngine;

namespace Paradise.Client {
	[HarmonyPatch(typeof(UserInput))]
	public class UserInputHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(UserInputHook));

		static UserInputHook() {
			Log.Info($"[{nameof(UserInputHook)}] hooking {nameof(UserInput)}");
		}

		[HarmonyPatch("UpdateMouse"), HarmonyPrefix]
		public static bool UpdateMouse_Prefix() {
			if (Camera.main != null) {
				float fovMult = Mathf.Pow(Camera.main.fieldOfView / ApplicationDataManager.ApplicationOptions.CameraFovMax, 1.1f);

				UserInput.Mouse.x += AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.HorizontalLook) * ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity * fovMult;
				UserInput.Mouse.x = ClampAngle(UserInput.Mouse.x, ApplicationDataManager.ApplicationOptions.InputMouseRotationMinX, ApplicationDataManager.ApplicationOptions.InputMouseRotationMaxX);

				int mouseMult = (!ApplicationDataManager.ApplicationOptions.InputInvertMouse) ? 1 : -1;

				UserInput.Mouse.y += AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.VerticalLook) * ApplicationDataManager.ApplicationOptions.InputYMouseSensitivity * mouseMult * fovMult;
				UserInput.Mouse.y = ClampAngle(UserInput.Mouse.y, ApplicationDataManager.ApplicationOptions.InputMouseRotationMinY, ApplicationDataManager.ApplicationOptions.InputMouseRotationMaxY);
			}

			return false;
		}

		private static float ClampAngle(float angle, float min, float max) {
			if (angle < -360f) {
				angle += 360f;
			}
			if (angle > 360f) {
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}
	}
}
