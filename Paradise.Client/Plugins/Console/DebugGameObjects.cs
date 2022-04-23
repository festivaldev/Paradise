using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	class DebugGameObjects : IDebugPage {
		public string Title => "GameObjects";

		private bool m_bindingFlagPublic = true;
		private bool m_bindingFlagNonPublic = true;
		private bool m_bindingFlagStatic = true;
		private Vector2 m_inspectScrollPosition;
		private Dictionary<Component, bool> m_inspectOpen = new Dictionary<Component, bool>();
		private Dictionary<Transform, bool> m_hierarchyOpen = new Dictionary<Transform, bool>();
		private Vector2 m_hierarchyScrollPosition;
		private Transform m_inspect;
		private List<Transform> m_rootTransforms = new List<Transform>();
		private string m_search = "";

		public void Draw() {
			GUILayout.BeginHorizontal("box");
			m_hierarchyScrollPosition = GUILayout.BeginScrollView(m_hierarchyScrollPosition, false, true, GUILayout.Width(Math.Min(600f, Screen.width / 3)), GUILayout.Height(Screen.height - 40));

			GUILayout.Label("Hierarchy");

			GUILayout.BeginHorizontal();
			m_search = GUILayout.TextField(m_search);
			if (GUILayout.Button("Search", GUILayout.Width(60f))) {
				Search(m_search);
			}
			//GUILayout.Label("Filter");
			//GUILayout.Space(8f);
			//m_search = GUILayout.TextField(m_search);
			GUILayout.EndHorizontal();

			foreach (Transform transform in m_rootTransforms) {
				ShowHierarchy(transform);
			}

			GUILayout.EndScrollView();


			if (m_inspect != null) {
				m_inspectScrollPosition = GUILayout.BeginScrollView(m_inspectScrollPosition, false, true, GUILayout.Width(300f), GUILayout.Height(Screen.height - 40));
				GUILayout.BeginHorizontal();
				GUILayout.Label(m_inspect.name);
				if (GUILayout.Button("x", GUILayout.Width(20f))) {
					m_inspect = null;
				}
				GUILayout.EndHorizontal();

				ShowInspect(m_inspect);

				GUILayout.EndScrollView();
			}
			GUILayout.EndHorizontal();
		}

		private void GenericsGUI(Component component, BindingFlags flags) {
			FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields.Concat(component.GetType().GetFields(BindingFlags.Public | BindingFlags.Static));
			fields.Concat(component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
			fields.Concat(component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Static));

			foreach (FieldInfo fieldInfo in fields) {
				try {
					object value = fieldInfo.GetValue(component);
					string str = value.ToString();
					if (fieldInfo.FieldType.IsArray || fieldInfo.FieldType.GetInterface("IEnumerable") != null) {
						GUILayout.Label(fieldInfo.Name);

						var index = 0;
						foreach (var item in (IEnumerable)value) {
							GUILayout.Label($"[{index}] {item}");
							index++;
						}
					} else if (value is bool) {
						GUILayout.Label(fieldInfo.Name);
						bool flag = GUILayout.Toggle((bool)value, str);
						fieldInfo.SetValue(component, flag);
					} else if (value is string) {
						GUILayout.Label(fieldInfo.Name);
						string value2 = GUILayout.TextField((string)value);
						fieldInfo.SetValue(component, value2);
					} else if (value is int) {
						GUILayout.Label(fieldInfo.Name);
						int num = Convert.ToInt32(GUILayout.TextField(value.ToString()));
						fieldInfo.SetValue(component, num);
					} else if (value is float) {
						GUILayout.Label(fieldInfo.Name);
						float num2 = (float)Convert.ToDouble(GUILayout.TextField(value.ToString()));
						fieldInfo.SetValue(component, num2);
					} else {
						GUILayout.Label(fieldInfo.Name + ": " + str);
					}
				} catch (Exception) {
					GUILayout.Label(fieldInfo.Name);
				}
			}

			PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			properties.Concat(component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static));
			properties.Concat(component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));
			properties.Concat(component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Static));

			foreach (var propertyInfo in properties) {
				try {
					var value = propertyInfo.GetValue(component, null);
					var valueString = value.ToString();


					if (value is bool) {
						GUILayout.Label(propertyInfo.Name);

						var _value = GUILayout.Toggle((bool)value, valueString);
						propertyInfo.SetValue(component, _value, null);
					} else if (value is string) {
						GUILayout.Label(propertyInfo.Name);

						var _value = GUILayout.TextField(valueString);
						propertyInfo.SetValue(component, _value, null);
					} else if (value is int) {
						GUILayout.Label(propertyInfo.Name);

						var _value = Convert.ToInt32(GUILayout.TextField(valueString));
						propertyInfo.SetValue(component, _value, null);
					} else if (value is float) {
						GUILayout.Label(propertyInfo.Name);

						var _value = (float)Convert.ToDouble(GUILayout.TextField(valueString));
						propertyInfo.SetValue(component, _value, null);
					} else if (value is UnityEngine.Transform) {
						TransformGUI(component);
					} else if (value is UnityEngine.GameObject) {
						GUILayout.Label($"{propertyInfo.Name} is gameobject");
						//GenericsGUI(component, flags);
					} else {
						GUILayout.Label($"{propertyInfo.Name}: {valueString}");
					}
				} catch (Exception e) {
					//GUILayout.Label(propertyInfo.Name);
				}
			}
		}

		private void ScriptGUI(Component component) {
			GUILayout.Label("Jonas tinkt");
			GUILayout.Label(component.name);
		}

		private void Search(string keyword) {
			m_hierarchyOpen.Clear();

			UnityEngine.Object.FindObjectOfType<GameObject>();
			if (string.IsNullOrEmpty(keyword)) {
				m_rootTransforms = (from x in UnityEngine.Object.FindObjectsOfType<Transform>()
									where x.parent == null
									select x).ToList<Transform>();
			} else {
				m_rootTransforms = (from x in UnityEngine.Object.FindObjectsOfType<Transform>()
									where x.name.Contains(keyword)
									select x).ToList<Transform>();
			}

			//m_rootTransforms.Sort(new Comparison<Transform>(Inspector.TransformNameAscendingSort));
		}

		private void ShowInspect(Transform transform) {
			if (transform != null) {
				GUI.enabled = transform.parent != null;
				if (GUILayout.Button("Go to Parent")) {
					m_inspect = transform.parent;
					return;
				}
				GUI.enabled = true;

				transform.gameObject.SetActive(GUILayout.Toggle(transform.gameObject.activeSelf, "Enabled"));
				GUILayout.Label($"Layer: {LayerMask.LayerToName(transform.gameObject.layer)}");

				m_bindingFlagPublic = GUILayout.Toggle(m_bindingFlagPublic, "Show public");
				m_bindingFlagNonPublic = GUILayout.Toggle(m_bindingFlagNonPublic, "Show non-public");
				m_bindingFlagStatic = GUILayout.Toggle(m_bindingFlagStatic, "Show static");

				BindingFlags bindingFlags = m_bindingFlagPublic ? BindingFlags.Public : BindingFlags.Default;
				bindingFlags |= (m_bindingFlagNonPublic ? BindingFlags.NonPublic : BindingFlags.Default);
				bindingFlags |= (m_bindingFlagStatic ? BindingFlags.Static : BindingFlags.Default);

				foreach (Component component in transform.GetComponents<Component>()) {
					GUILayout.BeginHorizontal();

					Type type = component.GetType();
					GUILayout.Label($"<{type}>");

					if (GUILayout.Button(m_inspectOpen.ContainsKey(component) && m_inspectOpen[component] ? "-" : "+", GUILayout.Width(20f))) {
						if (!m_inspectOpen.ContainsKey(component)) {
							m_inspectOpen[component] = true;
						} else {
							m_inspectOpen[component] = !m_inspectOpen[component];
						}
					}

					GUILayout.EndHorizontal();

					if (m_inspectOpen.ContainsKey(component) && m_inspectOpen[component]) {
						GUILayout.BeginVertical("box");

						if (component is Transform) {
							TransformGUI(component);
						} else {
							GenericsGUI(component, bindingFlags);
						}

						GUILayout.EndVertical();
					}
				}

				GenericsGUI(transform, BindingFlags.Default);
			}
		}

		private void ShowHierarchy(Transform transform) {
			if (transform == null) return;

			GUILayout.BeginHorizontal("box");
			GUILayout.Label(transform.name);
			if (GUILayout.Button("i", GUILayout.Width(20f))) {
				m_inspect = transform;
				m_inspectOpen.Clear();
			}

			GUI.enabled = transform.childCount > 0;
			if (GUILayout.Button(m_hierarchyOpen.ContainsKey(transform) && m_hierarchyOpen[transform] ? "-" : "+", GUILayout.Width(20f))) {
				if (!m_hierarchyOpen.ContainsKey(transform)) {
					m_hierarchyOpen[transform] = true;
				} else {
					m_hierarchyOpen[transform] = !m_hierarchyOpen[transform];
				}
			}
			GUI.enabled = true;

			GUILayout.EndHorizontal();
			if (m_hierarchyOpen.ContainsKey(transform) && m_hierarchyOpen[transform]) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(20f);
				GUILayout.BeginVertical();
				foreach (object obj in transform) {
					Transform transform2 = (Transform)obj;
					ShowHierarchy(transform2);
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		private void TransformGUI(Component component) {
			GUILayout.Label($"Tag: {component.gameObject.tag}");

			Transform transform = (Transform)component;

			GUILayout.Label($"Position:");
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			Vector3 vector = transform.localPosition;
			vector.x = (float)Convert.ToDouble(GUILayout.TextField(vector.x.ToString()));
			vector.y = (float)Convert.ToDouble(GUILayout.TextField(vector.y.ToString()));
			vector.z = (float)Convert.ToDouble(GUILayout.TextField(vector.z.ToString()));
			transform.localPosition = vector;
			GUILayout.EndHorizontal();

			GUILayout.Label($"Rotation:");
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			vector = transform.localRotation.eulerAngles;
			vector.x = (float)Convert.ToDouble(GUILayout.TextField(vector.x.ToString()));
			vector.y = (float)Convert.ToDouble(GUILayout.TextField(vector.y.ToString()));
			vector.z = (float)Convert.ToDouble(GUILayout.TextField(vector.z.ToString()));
			transform.localRotation = Quaternion.Euler(vector);
			GUILayout.EndHorizontal();

			GUILayout.Label("Scale:");
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			vector = transform.localScale;
			vector.x = (float)Convert.ToDouble(GUILayout.TextField(vector.x.ToString()));
			vector.y = (float)Convert.ToDouble(GUILayout.TextField(vector.y.ToString()));
			vector.z = (float)Convert.ToDouble(GUILayout.TextField(vector.z.ToString()));
			transform.localScale = vector;
			transform.gameObject.isStatic = false;
			GUILayout.EndHorizontal();
		}
	}
}
