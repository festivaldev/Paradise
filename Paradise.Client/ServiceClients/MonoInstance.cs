using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Paradise.Client {
	internal class MonoInstance : MonoBehaviour {
		public static MonoBehaviour Mono {
			get {
				if (MonoInstance.mono == null) {
					GameObject gameObject = GameObject.Find("AutoMonoBehaviours");
					if (gameObject == null) {
						gameObject = new GameObject("AutoMonoBehaviours");
					}
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					MonoInstance.mono = gameObject.AddComponent<MonoInstance>();
				}
				return MonoInstance.mono;
			}
		}

		private static MonoBehaviour mono;
	}
}
