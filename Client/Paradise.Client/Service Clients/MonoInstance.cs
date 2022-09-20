using UnityEngine;

namespace Paradise.Client {
	internal class MonoInstance : MonoBehaviour {
		private static MonoBehaviour mono;

		public static MonoBehaviour Mono {
			get {
				if (mono == null) {
					GameObject gameObject = GameObject.Find("AutoMonoBehaviours");

					if (gameObject == null) {
						gameObject = new GameObject("AutoMonoBehaviours");
					}

					DontDestroyOnLoad(gameObject);
					mono = gameObject.AddComponent<MonoInstance>();
				}
				return mono;
			}
		}
	}
}
