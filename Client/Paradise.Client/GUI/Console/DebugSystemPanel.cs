using UnityEngine;

namespace Paradise.Client {
	internal class DebugSystemPanel : IDebugPage {
		public string Title => "System Info";

		public void Draw() {
			ParadiseGUITools.DrawGroup("System Info", delegate {
				ParadiseGUITools.DrawTextField("Device Name", SystemInfo.deviceName);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Device Model", SystemInfo.deviceModel);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Device Identifier", SystemInfo.deviceUniqueIdentifier);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Device Type", SystemInfo.deviceType);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Operating System", SystemInfo.operatingSystem);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("CPU Type", SystemInfo.processorType);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("CPU Core Count", SystemInfo.processorCount);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Usable Memory", $"{SystemInfo.systemMemorySize} MB");

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Toggle(SystemInfo.supportsAccelerometer, "Supports Accelerometer", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsGyroscope, "Supports Gyroscope", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsLocationService, "Supports Location Services", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsVibration, "Supports Vibration", BlueStonez.toggle);

			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("GPU Info", delegate {
				ParadiseGUITools.DrawTextField("Manufacturer", SystemInfo.graphicsDeviceVendor);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Model", SystemInfo.graphicsDeviceName);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Vendor ID", $"{SystemInfo.graphicsDeviceVendorID:x4}");
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Product ID", $"{SystemInfo.graphicsDeviceID:x4}");
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Driver Version", SystemInfo.graphicsDeviceVersion);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Usable Memory", $"{SystemInfo.graphicsMemorySize} MB");
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Shader Level", SystemInfo.graphicsShaderLevel);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Pixel Fill Rate", SystemInfo.graphicsPixelFillrate);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Supported Render Target Count", SystemInfo.supportedRenderTargetCount);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Max Texture Size", SystemInfo.maxTextureSize);

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Toggle(SystemInfo.supports3DTextures, "Supports 3D Textures", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsComputeShaders, "Supports Compute Shaders", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsImageEffects, "Supports Image Effects", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsInstancing, "Supports Instancing", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsRenderTextures, "Supports Render Textures", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsRenderToCubemap, "Supports Render to Cubemap", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsShadows, "Supports Shadows", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsSparseTextures, "Supports Sparse Textures", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Supports Stencil", SystemInfo.supportsStencil);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				GUILayout.Toggle(SystemInfo.supportsVertexPrograms, "Supports Vertex Programs", BlueStonez.toggle);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("NPOT Support", SystemInfo.npotSupport);
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Graphics Overrides", delegate {
				var pixelLightCount = (float)QualitySettings.pixelLightCount;
				ParadiseGUITools.DrawSlider("Pixel Light Count", ref pixelLightCount, 0, 10, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (pixelLightCount != QualitySettings.pixelLightCount) {
					QualitySettings.pixelLightCount = Mathf.RoundToInt(pixelLightCount);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var masterTextureLimit = (float)QualitySettings.masterTextureLimit;
				ParadiseGUITools.DrawSlider("Master Texture Limit", ref masterTextureLimit, 0, 10, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (masterTextureLimit != QualitySettings.masterTextureLimit) {
					QualitySettings.masterTextureLimit = Mathf.RoundToInt(masterTextureLimit);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var maxQueuedFrames = (float)QualitySettings.maxQueuedFrames;
				ParadiseGUITools.DrawSlider("Max Queued Frames", ref maxQueuedFrames, -1, 10, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (maxQueuedFrames != QualitySettings.maxQueuedFrames) {
					QualitySettings.maxQueuedFrames = Mathf.RoundToInt(maxQueuedFrames);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var maxLODLevel = (float)QualitySettings.maximumLODLevel;
				ParadiseGUITools.DrawSlider("Max LOD Level", ref maxLODLevel, 0, 7, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (maxLODLevel != QualitySettings.maximumLODLevel) {
					QualitySettings.maximumLODLevel = Mathf.RoundToInt(maxLODLevel);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var vsync = (float)QualitySettings.vSyncCount;
				ParadiseGUITools.DrawSlider("Vertical Sync", ref vsync, 0, 2, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (vsync != QualitySettings.vSyncCount) {
					QualitySettings.vSyncCount = Mathf.RoundToInt(vsync);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var antiAliasing = (float)QualitySettings.antiAliasing;
				ParadiseGUITools.DrawSlider("Anti Aliasing", ref antiAliasing, 0, 4, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (antiAliasing != QualitySettings.antiAliasing) {
					QualitySettings.antiAliasing = Mathf.RoundToInt(antiAliasing);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var anisotropicFiltering = (float)QualitySettings.anisotropicFiltering;
				ParadiseGUITools.DrawSlider("Anisotropic Filtering", ref anisotropicFiltering, 0, 2, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (anisotropicFiltering != (int)QualitySettings.anisotropicFiltering) {
					QualitySettings.anisotropicFiltering = (AnisotropicFiltering)Mathf.RoundToInt(anisotropicFiltering);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var lodBias = (float)QualitySettings.lodBias;
				ParadiseGUITools.DrawSlider("LOD Bias", ref lodBias, 0, 4, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (lodBias != QualitySettings.lodBias) {
					QualitySettings.lodBias = Mathf.RoundToInt(lodBias);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var globalMaxLOD = (float)Shader.globalMaximumLOD;
				ParadiseGUITools.DrawSlider("Global Max LOD", ref globalMaxLOD, 100, 600, delegate (float value) {
					GUILayout.Label(Mathf.RoundToInt(value).ToString(), BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (globalMaxLOD != Shader.globalMaximumLOD) {
					Shader.globalMaximumLOD = Mathf.RoundToInt(globalMaxLOD);
				}
			});
		}
	}
}
