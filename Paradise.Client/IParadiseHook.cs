using System;

namespace Paradise.Client {
	public interface IParadiseHook {
		void Hook(HarmonyLib.Harmony harmonyInstance);
	}
}
