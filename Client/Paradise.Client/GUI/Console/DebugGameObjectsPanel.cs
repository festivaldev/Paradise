using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugGameObjectsPanel : IDebugPage {
		private const float UPDATE_INTERVAL = 1f;

		public string Title => "GameObjects";

		private List<Transform> rootTransforms = new List<Transform>();
		private float transformUpdateTime;
		private string searchString = string.Empty;

		private readonly List<Transform> history = new List<Transform> { };
		private Transform InspectedObject => history.LastOrDefault();
		private ParadiseTraverse traverse;

		private readonly Dictionary<Transform, bool> hierarchyOpen = new Dictionary<Transform, bool>();
		private readonly Dictionary<Component, bool> inspectOpen = new Dictionary<Component, bool>();

		private bool showUnknownFields;
		private bool showInvalidFields;

		public void Draw() {
			if (InspectedObject == null) {
				if (Time.time - transformUpdateTime >= UPDATE_INTERVAL) {
					rootTransforms = UnityEngine.Object.FindObjectsOfType<Transform>().Where(_ => _.parent == null).ToList();
					transformUpdateTime = Time.time;
				}

				ParadiseGUITools.DrawGroup("Game Objects", delegate {
					ParadiseGUITools.DrawTextField("Filter", ref searchString);

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					GUI.enabled = hierarchyOpen.Values.Where(_ => _).Count() > 0;
					if (GUILayout.Button("Collapse all", BlueStonez.buttondark_small, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
						hierarchyOpen.Clear();
					}
					GUI.enabled = true;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					var filteredRootTransforms = rootTransforms.Where(_ => string.IsNullOrEmpty(searchString) || _.name.ToLower().Contains(searchString.ToLower()));
					if (filteredRootTransforms.Count() > 0) {
						foreach (var transform in filteredRootTransforms) {
							ShowHierarchy(transform);
						}
					} else {
						GUI.enabled = false;
						GUILayout.Label($"No results for \"{searchString}\".");
						GUI.enabled = true;
					}
				});
			} else {
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Back", BlueStonez.buttondark_small, GUILayout.Width(48f), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
					PopHistory();
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

				ShowInspect(InspectedObject);
			}
		}

		private void ShowHierarchy(Transform transform) {
			GUILayout.BeginHorizontal();

			GUI.enabled = transform.childCount > 0;
			if (GUILayout.Button(hierarchyOpen.ContainsKey(transform) && hierarchyOpen[transform] ? "-" : "+", BlueStonez.buttondark_small, GUILayout.Width(ParadiseGUITools.BUTTON_HEIGHT), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
				if (!hierarchyOpen.ContainsKey(transform)) {
					hierarchyOpen[transform] = true;
				} else {
					hierarchyOpen[transform] = !hierarchyOpen[transform];
				}
			}
			GUI.enabled = true;

			GUILayout.Space(8f);

			GUILayout.Label(transform.name, BlueStonez.label_interparkbold_11pt_left, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT));

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("i", BlueStonez.buttondark_small, GUILayout.Width(ParadiseGUITools.BUTTON_HEIGHT), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
				PushHistory(transform);
			}

			GUILayout.EndHorizontal();

			GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

			if (hierarchyOpen.ContainsKey(transform) && hierarchyOpen[transform]) {
				GUILayout.BeginHorizontal();
				GUILayout.Space(ParadiseGUITools.BUTTON_HEIGHT + 8f);
				GUILayout.BeginVertical();

				foreach (object obj in transform) {
					Transform transform2 = (Transform)obj;
					ShowHierarchy(transform2);
				}

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		private void ShowInspect(Transform transform) {
			ParadiseGUITools.DrawGroup("General", delegate {
				GUI.enabled = transform.parent != null;
				if (GUILayout.Button("Go to Parent", BlueStonez.buttondark_small, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
					PushHistory(transform.parent);

					return;
				}
				GUI.enabled = true;

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var active = GUILayout.Toggle(transform.gameObject.activeSelf, "Is Active", BlueStonez.toggle);
				if (active != transform.gameObject.activeSelf) {
					transform.gameObject.SetActive(active);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label($"Layer: {LayerMask.LayerToName(transform.gameObject.layer)}");

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				showInvalidFields = GUILayout.Toggle(showInvalidFields, "Show invalid fields & properties");
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				showUnknownFields = GUILayout.Toggle(showUnknownFields, "Show unknown fields & properties");
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Position & Rotation", delegate {
				GUILayout.Label("Position", BlueStonez.label_interparkbold_11pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				GUILayout.BeginHorizontal();

				var position = transform.localPosition;

				var posX = GUILayout.TextField(position.x.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(posX) && Convert.ToDouble(posX) != position.x) {
					position.x = (float)Convert.ToDouble(posX);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var posY = GUILayout.TextField(position.y.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(posY) && Convert.ToDouble(posY) != position.y) {
					position.y = (float)Convert.ToDouble(posY);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var posZ = GUILayout.TextField(position.z.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(posZ) && Convert.ToDouble(posZ) != position.z) {
					position.z = (float)Convert.ToDouble(posZ);
				}

				if (position != transform.localPosition) {
					transform.localPosition = position;
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label("Rotation", BlueStonez.label_interparkbold_11pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				GUILayout.BeginHorizontal();

				var rotation = transform.localRotation.eulerAngles;

				var rotX = GUILayout.TextField(rotation.x.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(rotX) && Convert.ToDouble(rotX) != rotation.x) {
					rotation.x = (float)Convert.ToDouble(rotX);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var rotY = GUILayout.TextField(rotation.y.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(rotY) && Convert.ToDouble(rotY) != rotation.y) {
					rotation.y = (float)Convert.ToDouble(rotY);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var rotZ = GUILayout.TextField(rotation.z.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(rotZ) && Convert.ToDouble(rotZ) != rotation.z) {
					rotation.z = (float)Convert.ToDouble(rotZ);
				}

				if (rotation != transform.localRotation.eulerAngles) {
					transform.localRotation = Quaternion.Euler(rotation);
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label("Scale", BlueStonez.label_interparkbold_11pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				GUILayout.BeginHorizontal();

				var scale = transform.localScale;

				var scaleX = GUILayout.TextField(scale.x.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(scaleX) && Convert.ToDouble(scaleX) != scale.x) {
					scale.x = (float)Convert.ToDouble(scaleX);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var scaleY = GUILayout.TextField(scale.y.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(scaleY) && Convert.ToDouble(scaleY) != scale.y) {
					scale.y = (float)Convert.ToDouble(scaleY);
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var scaleZ = GUILayout.TextField(scale.z.ToString("F2"), BlueStonez.textField);
				if (!string.IsNullOrEmpty(scaleZ) && Convert.ToDouble(scaleZ) != scale.z) {
					scale.z = (float)Convert.ToDouble(scaleZ);
				}

				if (scale != transform.localScale) {
					transform.localScale = scale;
				}

				GUILayout.EndHorizontal();
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Fields & Properties", delegate {
				GenericsGUI(transform);
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Components", delegate {
				foreach (Component component in transform.GetComponents<Component>()) {
					if (component is Transform) continue;

					GUILayout.BeginHorizontal();

					Type type = component.GetType();
					GUILayout.Label($"<{type}>");

					if (GUILayout.Button(inspectOpen.ContainsKey(component) && inspectOpen[component] ? "-" : "+", BlueStonez.buttondark_small, GUILayout.Width(ParadiseGUITools.BUTTON_HEIGHT), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
						if (!inspectOpen.ContainsKey(component)) {
							inspectOpen[component] = true;
						} else {
							inspectOpen[component] = !inspectOpen[component];
						}
					}

					GUILayout.EndHorizontal();

					if (inspectOpen.ContainsKey(component) && inspectOpen[component]) {
						GenericsGUI(component);
					}
				}
			});
		}

		private void GenericsGUI(Component component) {
			FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields.Concat(component.GetType().GetFields(BindingFlags.Public | BindingFlags.Static));
			fields.Concat(component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
			fields.Concat(component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Static));

			foreach (FieldInfo fieldInfo in fields) {
				try {
					var value = fieldInfo.GetValue(component);
					var valueString = value.ToString();

					if (value is bool) {
						var _value = GUILayout.Toggle((bool)value, fieldInfo.Name);
						if (_value != (bool)value) {
							fieldInfo.SetValue(component, _value);
						}
					} else if (value is int) {
						var _value = value.ToString();
						ParadiseGUITools.DrawTextField(fieldInfo.Name, ref _value);

						if (_value != valueString) {
							fieldInfo.SetValue(component, Convert.ToInt32(_value));
						}
					} else if (value is float) {
						var _value = ((float)value).ToString("F2");
						ParadiseGUITools.DrawTextField(fieldInfo.Name, ref _value);

						if (_value != valueString) {
							fieldInfo.SetValue(component, (float)Convert.ToDouble(_value));
						}
					} else if (value is string) {
						var _value = value.ToString();
						ParadiseGUITools.DrawTextField(fieldInfo.Name, ref _value);

						if (_value != valueString) {
							fieldInfo.SetValue(component, _value);
						}
						//} else if (value is UnityEngine.Transform) {

					} else if (value is UnityEngine.GameObject) {
						GUILayout.BeginHorizontal();

						GUILayout.Label(fieldInfo.Name, BlueStonez.label_interparkbold_11pt);
						GUILayout.FlexibleSpace();

						GUI.enabled = value != null;
						if (GUILayout.Button("i", BlueStonez.buttondark_small, GUILayout.Width(ParadiseGUITools.BUTTON_HEIGHT), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
							PushHistory(((GameObject)value).transform);
						}
						GUI.enabled = true;

						GUILayout.EndHorizontal();
					} else {
						if (!showUnknownFields) continue;
						GUILayout.Label($"{fieldInfo.Name}<{value.GetType()}>: {valueString}");
					}
				} catch {
					if (!showInvalidFields) continue;
					GUILayout.Label($"{fieldInfo.Name} (invalid)");
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
			}


			PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			properties.Concat(component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static));
			properties.Concat(component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));
			properties.Concat(component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Static));

			foreach (var propertyInfo in properties) {
				try {
					var value = propertyInfo.GetValue(component, null);
					var valueString = value.ToString();

					if (value is bool v1) {
						var _value = GUILayout.Toggle(v1, propertyInfo.Name);
						if (_value != v1) {
							propertyInfo.SetValue(component, _value, null);
						}
					} else if (value is int) {
						var _value = value.ToString();
						ParadiseGUITools.DrawTextField(propertyInfo.Name, ref _value);

						if (_value != valueString) {
							propertyInfo.SetValue(component, Convert.ToInt32(_value), null);
						}
					} else if (value is float v) {
						var _value = v.ToString("F2");
						ParadiseGUITools.DrawTextField(propertyInfo.Name, ref _value);

						if (_value != valueString) {
							propertyInfo.SetValue(component, (float)Convert.ToDouble(_value), null);
						}
					} else if (value is string) {
						var _value = value.ToString();
						ParadiseGUITools.DrawTextField(propertyInfo.Name, ref _value);

						if (_value != valueString) {
							propertyInfo.SetValue(component, _value, null);
						}
						//} else if (value is UnityEngine.Transform) {

					} else if (value is UnityEngine.GameObject) {
						GUILayout.BeginHorizontal();

						GUILayout.Label(propertyInfo.Name, BlueStonez.label_interparkbold_11pt);
						GUILayout.FlexibleSpace();

						GUI.enabled = value != null;
						if (GUILayout.Button("i", BlueStonez.buttondark_small, GUILayout.Width(ParadiseGUITools.BUTTON_HEIGHT), GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
							PushHistory(((GameObject)value).transform);
						}
						GUI.enabled = true;

						GUILayout.EndHorizontal();
					} else {
						if (!showUnknownFields) continue;
						GUILayout.Label($"{propertyInfo.Name}<{value.GetType()}>: {valueString}");
					}
				} catch {
					if (!showInvalidFields) continue;
					GUILayout.Label($"{propertyInfo.Name} (invalid)");
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
			}
		}

		private void PushHistory(Transform transform) {
			history.Add(transform);
			traverse = ParadiseTraverse.Create(transform);

			inspectOpen.Clear();
		}

		private void PopHistory() {
			if (history.Count == 0) return;

			history.RemoveAt(history.Count - 1);
			inspectOpen.Clear();

			if (InspectedObject != null) {
				traverse = ParadiseTraverse.Create(InspectedObject);
			} else {
				traverse = null;
			}
		}
	}
}
