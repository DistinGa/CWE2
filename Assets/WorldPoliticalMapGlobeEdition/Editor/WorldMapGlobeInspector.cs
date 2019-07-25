using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	[CustomEditor (typeof(WorldMapGlobe))]
	public class WorldMapGlobeInspector : Editor {
		const int CALCULATOR = 0;
		const int TICKERS = 1;
		const int DECORATOR = 2;
		const int EDITOR = 3;
		const string WPM_BUILD_HINT = "WPMBuildHint";
		WorldMapGlobe _map;
		Texture2D _headerTexture;
		string[] earthStyleOptions, frontiersDetailOptions, labelsQualityOptions, labelsRenderMethods, gridModeOptions, navigationModeOptions, zoomModeOptions, rotationAxisOptions;
		int[] earthStyleValues, gridModeValues, navigationModeValues, zoomModeValues, rotationAxisValues;
		GUIStyle blackBack, sectionHeaderNormalStyle;
		bool[] extracomp = new bool[4];
		bool expandUniverseSection, expandEarthSection, expandGridSection, expandTilesSection, expandCitiesSection, expandFoWSection, expandCountriesSection, expandProvincesSection, expandInteractionSection, expandDevicesSection;
		bool tileSizeComputed;
		SerializedProperty isDirty;
		float zoomLevel;

		void OnEnable () {
			_map = (WorldMapGlobe)target;
			_headerTexture = Resources.Load<Texture2D> ("EditorHeader");
			blackBack = new GUIStyle ();
			blackBack.normal.background = MakeTex (4, 4, Color.black);

			if (_map != null && _map.countries == null) {
				_map.Init ();
			}
			earthStyleOptions = new string[] {
				"Natural (2K, Unlit)", "Natural (2K, Standard Shader)", "Natural (2K, Scenic)", "Natural (2K, Scenic + City Lights)", "Alternate Style 1 (2K)", "Alternate Style 2 (2K)", "Alternate Style 3 (2K)", "Natural (8K, Unlit)", "Natural (8K, Standard Shader)", "Natural (8K Scenic)", "Natural (8K Scenic + City Lights)", "Natural (8K Scenic Scatter)", "Natural (8K Scenic Scatter + City Lights)",  "Natural (16K, Unlit)",  "Natural (16K Scenic)", "Natural (16K Scenic + City Lights)",  "Natural (16K Scenic Scatter)", "Natural (16K Scenic Scatter + City Lights)", "Solid Color", "Custom"
			};
			earthStyleValues = new int[] {
				(int)EARTH_STYLE.Natural, (int)EARTH_STYLE.StandardShader2K, (int)EARTH_STYLE.Scenic, (int)EARTH_STYLE.ScenicCityLights, (int)EARTH_STYLE.Alternate1, (int)EARTH_STYLE.Alternate2, (int)EARTH_STYLE.Alternate3,  (int)EARTH_STYLE.NaturalHighRes, (int)EARTH_STYLE.StandardShader8K, (int)EARTH_STYLE.NaturalHighResScenic, (int)EARTH_STYLE.NaturalHighResScenicCityLights, (int)EARTH_STYLE.NaturalHighResScenicScatter, (int)EARTH_STYLE.NaturalHighResScenicScatterCityLights, (int)EARTH_STYLE.NaturalHighRes16K, (int)EARTH_STYLE.NaturalHighRes16KScenic, (int)EARTH_STYLE.NaturalHighRes16KScenicCityLights, (int)EARTH_STYLE.NaturalHighRes16KScenicScatter, (int)EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights, (int)EARTH_STYLE.SolidColor, (int)EARTH_STYLE.Custom
			};
			
			frontiersDetailOptions = new string[] {
				"Low",
				"High"
			};
			labelsQualityOptions = new string[] {
				"Low (2048x1024)",
				"Medium (4096x2048)",
				"High (8192x4096)"
			};
			labelsRenderMethods = new string[] {
				"Blended",
				"World Space"
			};
			gridModeOptions = new string[] {
				"Overlay",
				"Masked"
			};
			gridModeValues = new int[] {
				(int)GRID_MODE.OVERLAY, (int)GRID_MODE.MASKED
			};
			
			navigationModeOptions = new string[] {
				"Earth Rotates",
				"Camera Rotates"
			};
			navigationModeValues = new int[] {
				(int)NAVIGATION_MODE.EARTH_ROTATES, (int)NAVIGATION_MODE.CAMERA_ROTATES
			};
			zoomModeOptions = new string[] {
				"Camera Moves",
				"Earth Moves"
			};
			zoomModeValues = new int[] {
				(int)ZOOM_MODE.CAMERA_MOVES, (int)ZOOM_MODE.EARTH_MOVES
			};
			rotationAxisOptions = new string[] { "Both", "X-Axis", "Y-Axis" };
			rotationAxisValues = new int[] {
				(int)ROTATION_AXIS_ALLOWED.BOTH_AXIS,
				(int)ROTATION_AXIS_ALLOWED.X_AXIS_ONLY,
				(int)ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY
			};

			expandUniverseSection = EditorPrefs.GetBool ("WPMGlobeUniverseExpand", false);
			expandEarthSection = EditorPrefs.GetBool ("WPMGlobeEarthExpand", false);
			expandGridSection = EditorPrefs.GetBool ("WPMGlobeGridExpand", false);
			expandTilesSection = EditorPrefs.GetBool ("WPMGlobeTilesExpand", false);
			expandCitiesSection = EditorPrefs.GetBool ("WPMGlobeCitiesExpand", false);
			expandFoWSection = EditorPrefs.GetBool ("WPMGlobeFoWExpand", false);
			expandCountriesSection = EditorPrefs.GetBool ("WPMGlobeCountriesExpand", false);
			expandProvincesSection = EditorPrefs.GetBool ("WPMGlobeProvincesExpand", false);
			expandInteractionSection = EditorPrefs.GetBool ("WPMGlobeInteractionExpand", false);
			expandDevicesSection = EditorPrefs.GetBool ("WPMGlobeDevicesExpand", false);

			UpdateExtraComponentStatus ();

			isDirty = serializedObject.FindProperty ("isDirty");
			zoomLevel = Mathf.Clamp(_map.GetZoomLevel (), 0, 5f);
		}

		void OnDisable () {
			EditorPrefs.SetBool ("WPMGlobeUniverseExpand", expandUniverseSection);
			EditorPrefs.SetBool ("WPMGlobeEarthExpand", expandEarthSection);
			EditorPrefs.SetBool ("WPMGlobeGridExpand", expandGridSection);
			EditorPrefs.SetBool ("WPMGlobeTilesExpand", expandTilesSection);
			EditorPrefs.SetBool ("WPMGlobeCitiesExpand", expandCitiesSection);
			EditorPrefs.SetBool ("WPMGlobeFoWExpand", expandFoWSection);
			EditorPrefs.SetBool ("WPMGlobeCountriesExpand", expandCountriesSection);
			EditorPrefs.SetBool ("WPMGlobeProvincesExpand", expandProvincesSection);
			EditorPrefs.SetBool ("WPMGlobeInteractionExpand", expandInteractionSection);
			EditorPrefs.SetBool ("WPMGlobeDevicesExpand", expandDevicesSection);
		}

		void UpdateExtraComponentStatus () {
			extracomp [CALCULATOR] = _map.gameObject.GetComponent<WorldMapCalculator> () != null;
			extracomp [TICKERS] = _map.gameObject.GetComponent<WorldMapTicker> () != null;
			extracomp [DECORATOR] = _map.gameObject.GetComponent<WorldMapDecorator> () != null;
			extracomp [EDITOR] = _map.gameObject.GetComponent<WorldMapEditor> () != null;
		}

		public override void OnInspectorGUI () {
			if (_map == null || _map.countries == null) {
				OnEnable ();
				return;
			}

			if (EditorPrefs.GetInt (WPM_BUILD_HINT) == 0) {
				EditorPrefs.SetInt (WPM_BUILD_HINT, 1);
				EditorUtility.DisplayDialog ("World Political Map Globe Edition", "Thanks for purchasing!\nPlease read documentation for important tips about reducing application build size as this version includes many high resolution textures.\n\nFor additional help or questions please visit our Support Forum on kronnect.com\n\nWe hope you enjoy using WPM Globe Edition. Please consider rating WPM Globe Edition on the Asset Store.", "Ok");
			}


			if (_map.isDirty || (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "UndoRedoPerformed")) {
				#if UNITY_5_6_OR_NEWER
												serializedObject.UpdateIfRequiredOrScript ();
				#else
				serializedObject.UpdateIfDirtyOrScript ();
				#endif
			}

			if (sectionHeaderNormalStyle == null) {
				sectionHeaderNormalStyle = new GUIStyle (EditorStyles.foldout);
			}
			sectionHeaderNormalStyle.SetFoldoutColor ();

			EditorGUILayout.Separator ();
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
			GUILayout.BeginHorizontal (blackBack);
			GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
			GUILayout.EndHorizontal ();

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			
//												string sectionName = (_map.showMoon || _map.skyboxStyle != SKYBOX_STYLE.UserDefined || _map.sun != null) ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandUniverseSection = EditorGUILayout.Foldout (expandUniverseSection, "Universe Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandUniverseSection) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Skybox", GUILayout.Width (120));
				_map.skyboxStyle = (SKYBOX_STYLE)EditorGUILayout.EnumPopup (_map.skyboxStyle);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Sun GameObject", "Instead of setting a Sun Position manually, assign a Game Object (usually a Directional Light that acts as the Sun) to automatically synchronize the light direction."), GUILayout.Width (120));
				_map.sun = (Transform)EditorGUILayout.ObjectField (_map.sun, typeof(Transform), true);
				if (GUILayout.Button ("Flares?", GUILayout.Width (60))) {
					if (EditorUtility.DisplayDialog ("Sun Lens Flares FX", "For an additional Sun lens flares effect, including animated solar wind, we recommend using Beautify.\n\nBeautify is a full-screen image effect asset that enhances the image quality in real time and provides nice effects like anamorphic and Sun lens flares.", "More information", "Close")) {
						Application.OpenURL ("https://www.assetstore.unity3d.com/#!/content/61730?aid=1101lGsd");
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("   Sync Time of Day", "Aligns Earth and Sun rotation according to current daylight."), GUILayout.Width (120));
				_map.syncTimeOfDay = EditorGUILayout.Toggle (_map.syncTimeOfDay);
				EditorGUILayout.EndHorizontal ();
				if (_map.sun != null || _map.syncTimeOfDay)
					GUI.enabled = false;
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Sun Position", "Relative to the center of the Earth. Used for light direction calculation in scenic/scatter styles."), GUILayout.Width (120));
				_map.earthScenicLightDirection = EditorGUILayout.Vector3Field ("", _map.earthScenicLightDirection);
				EditorGUILayout.EndHorizontal ();
				GUI.enabled = true;
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Moon", GUILayout.Width (120));
				_map.showMoon = EditorGUILayout.Toggle (_map.showMoon);
				EditorGUILayout.EndHorizontal ();
				if (_map.showMoon) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Auto Scale", "Manages Moon position and scale automatically based on Earth dimensions."), GUILayout.Width (120));
					_map.moonAutoScale = EditorGUILayout.Toggle (_map.moonAutoScale);
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

//												labelStyle = _map.showEarth ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandEarthSection = EditorGUILayout.Foldout (expandEarthSection, "Earth & Atmosphere Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandEarthSection) {

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Earth", GUILayout.Width (120));
				_map.showEarth = EditorGUILayout.Toggle (_map.showEarth);

				if (GUILayout.Button ("Straighten")) {
					_map.StraightenGlobe (1.0f);
				}

				if (GUILayout.Button ("Tilt")) {
					_map.TiltGlobe ();
				}

				if (GUILayout.Button ("Redraw")) {
					_map.Redraw ();
				}

				EditorGUILayout.EndHorizontal ();

				if (_map.showEarth) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Earth Style", GUILayout.Width (120));
					_map.earthStyle = (EARTH_STYLE)EditorGUILayout.IntPopup ((int)_map.earthStyle, earthStyleOptions, earthStyleValues);

					if (_map.earthStyle == EARTH_STYLE.SolidColor) {
						GUILayout.Label ("Color");
						_map.earthColor = EditorGUILayout.ColorField (_map.earthColor);
					}
					EditorGUILayout.EndHorizontal ();

					if (_map.earthStyle.isScenic () || _map.earthStyle.isScatter ()) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Enable BumpMap", GUILayout.Width (120));
						_map.earthBumpMapEnabled = EditorGUILayout.Toggle (_map.earthBumpMapEnabled);
						EditorGUILayout.EndHorizontal ();

						if (_map.earthBumpMapEnabled) {
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label ("      Amount", GUILayout.Width (120));
							_map.earthBumpMapIntensity = EditorGUILayout.Slider (_map.earthBumpMapIntensity, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
						}

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Contrast", "Final Earth image contrast adjustment. Allows you to create more vivid images."), GUILayout.Width (120));
						_map.contrast = EditorGUILayout.Slider (_map.contrast, 0.5f, 1.5f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Brightness", "Final Earth image brightness adjustment."), GUILayout.Width (120));
						_map.brightness = EditorGUILayout.Slider (_map.brightness, 0f, 2f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Ambient", "Ambient light."), GUILayout.Width (120));
						_map.ambientLight = EditorGUILayout.Slider (_map.ambientLight, 0f, 1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Clouds", GUILayout.Width (120));
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("      Alpha", "Transparency of the cloud layer."), GUILayout.Width (120));
						_map.cloudsAlpha = EditorGUILayout.Slider (_map.cloudsAlpha, 0f, 1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Speed", GUILayout.Width (120));
						_map.cloudsSpeed = EditorGUILayout.Slider (_map.cloudsSpeed, -1f, 1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Elevation", GUILayout.Width (120));
						_map.cloudsElevation = EditorGUILayout.Slider (_map.cloudsElevation, 0.001f, 0.1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Shadows", GUILayout.Width (120));
						_map.cloudsShadowEnabled = EditorGUILayout.Toggle (_map.cloudsShadowEnabled);
						EditorGUILayout.EndHorizontal ();

						if (_map.cloudsShadowEnabled) {
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label ("         Strength", GUILayout.Width (120));
							_map.cloudsShadowStrength = EditorGUILayout.Slider (_map.cloudsShadowStrength, 0f, 1f);
							EditorGUILayout.EndHorizontal ();
						}

					}

					if (_map.earthStyle.isScenic ()) {

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Scenic Atmosphere", GUILayout.Width (120));
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Tint Color", GUILayout.Width (120));
						_map.atmosphereColor = EditorGUILayout.ColorField (_map.atmosphereColor);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Alpha", GUILayout.Width (120));
						_map.atmosphereAlpha = EditorGUILayout.Slider (_map.atmosphereAlpha, 0f, 1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Fall Off", GUILayout.Width (120));
						_map.atmosphereFallOff = EditorGUILayout.Slider (_map.atmosphereFallOff, 0f, 5f);
						EditorGUILayout.EndHorizontal ();
					}
				} else {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Double Sided", GUILayout.Width (120));
					_map.showBackSide = EditorGUILayout.Toggle (_map.showBackSide);
					EditorGUILayout.EndHorizontal ();
				}


				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Glow Intensity", GUILayout.Width (120));
				_map.earthScenicGlowIntensity = EditorGUILayout.Slider (_map.earthScenicGlowIntensity, 0, 2);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Phys. Based Glow", GUILayout.Width (120));
				_map.earthGlowScatter = EditorGUILayout.Toggle (_map.earthGlowScatter, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();

				if (!_map.earthGlowScatter) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Glow Thickness", GUILayout.Width (120));
					_map.atmosphereThickness = EditorGUILayout.Slider (_map.atmosphereThickness, 0.9f, 1.12f);
					EditorGUILayout.EndHorizontal ();
				}

				if (_map.earthStyle.isScenic ()) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Effect Intensity", GUILayout.Width (120));
					_map.earthScenicAtmosphereIntensity = EditorGUILayout.Slider (_map.earthScenicAtmosphereIntensity, 0, 1);
					EditorGUILayout.EndHorizontal ();
				}
				if (_map.showTiles || _map.earthStyle.isScatter () || _map.earthStyle.isScenic ()) {
					EditorGUILayout.HelpBox ("Inverted Mode is not compatible with Scenic/Scatter/Tile modes.", MessageType.Warning);
					GUI.enabled = false;
					_map.earthInvertedMode = false;
				}
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Inverted Mode", GUILayout.Width (120));
				_map.earthInvertedMode = EditorGUILayout.Toggle (_map.earthInvertedMode);
				EditorGUILayout.EndHorizontal ();
				GUI.enabled = true;

				if (!_map.showTiles) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   High Density Mesh", GUILayout.Width (120));
					_map.earthHighDensityMesh = EditorGUILayout.Toggle (_map.earthHighDensityMesh);
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandGridSection = EditorGUILayout.Foldout (expandGridSection, "Grid Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandGridSection) {
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Latitude Lines", GUILayout.Width (120));
				_map.showLatitudeLines = EditorGUILayout.Toggle (_map.showLatitudeLines, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Stepping", GUILayout.Width (120));
				_map.latitudeStepping = EditorGUILayout.IntSlider (_map.latitudeStepping, 5, 45);
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Longitude Lines", GUILayout.Width (120));
				_map.showLongitudeLines = EditorGUILayout.Toggle (_map.showLongitudeLines, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Stepping", GUILayout.Width (120));
				_map.longitudeStepping = EditorGUILayout.IntSlider (_map.longitudeStepping, 5, 45);
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Lines Color", GUILayout.Width (120));
				_map.gridLinesColor = EditorGUILayout.ColorField (_map.gridLinesColor);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Mode", GUILayout.Width (120));
				_map.gridMode = (GRID_MODE)EditorGUILayout.IntPopup ((int)_map.gridMode, gridModeOptions, gridModeValues, GUILayout.Width (130));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Hexagonal Grid", GUILayout.Width (120));
				_map.showHexagonalGrid = EditorGUILayout.Toggle (_map.showHexagonalGrid, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();
				if (_map.showHexagonalGrid) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Divisions", GUILayout.Width (120));
					_map.hexaGridDivisions = EditorGUILayout.IntSlider (_map.hexaGridDivisions, 15, 200);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Color", GUILayout.Width (120));
					float prevAlpha = _map.hexaGridColor.a;
					_map.hexaGridColor = EditorGUILayout.ColorField (_map.hexaGridColor);
					EditorGUILayout.EndHorizontal ();
					GUICheckTransparentColor (_map.hexaGridColor.a, prevAlpha);
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Use Mask", GUILayout.Width (120));
					_map.hexaGridUseMask = EditorGUILayout.Toggle (_map.hexaGridUseMask, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("      Mask Texture", GUILayout.Width (120));
					_map.hexaGridMask = (Texture2D)EditorGUILayout.ObjectField (_map.hexaGridMask, typeof(Texture2D), false);
					EditorGUILayout.EndHorizontal ();
					if (_map.hexaGridMask != null) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Height Threshold", GUILayout.Width (120));
						_map.hexaGridMaskThreshold = EditorGUILayout.IntSlider (_map.hexaGridMaskThreshold, 0, 255);
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Rotation Shift", GUILayout.Width (120));
					_map.hexaGridRotationShift = EditorGUILayout.Vector3Field ("", _map.hexaGridRotationShift);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Enable Highlight", GUILayout.Width (120));
					_map.hexaGridHighlightEnabled = EditorGUILayout.Toggle (_map.hexaGridHighlightEnabled);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Highlight Color", GUILayout.Width (120));
					_map.hexaGridHighlightColor = EditorGUILayout.ColorField (_map.hexaGridHighlightColor);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Highlight Speed", GUILayout.Width (120));
					_map.hexaGridHighlightSpeed = EditorGUILayout.Slider (_map.hexaGridHighlightSpeed, 0.1f, 5f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   PathFinding Method", GUILayout.Width (120));
					_map.pathFindingHeuristicFormula = (PathFinding.HeuristicFormula)EditorGUILayout.EnumPopup (_map.pathFindingHeuristicFormula);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Search Limit", GUILayout.Width (120));
					_map.pathFindingSearchLimit = EditorGUILayout.IntField (_map.pathFindingSearchLimit, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandTilesSection = EditorGUILayout.Foldout (expandTilesSection, "Tile System Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandTilesSection) {

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Tiles", GUILayout.Width (120));
				_map.showTiles = EditorGUILayout.Toggle (_map.showTiles, GUILayout.Width (40));

				if (_map.showTiles) {
					if (!Application.isPlaying)
						GUI.enabled = false;
					if (GUILayout.Button ("Reload Tiles")) {
						_map.ResetTiles ();
					}
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Server", GUILayout.Width (120));
					_map.tileServer = (TILE_SERVER)EditorGUILayout.IntPopup ((int)_map.tileServer, WorldMapGlobe.tileServerNames, WorldMapGlobe.tileServerValues);
					EditorGUILayout.EndHorizontal ();

					if (_map.tileServer == TILE_SERVER.Custom) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("Url Template", GUILayout.Width (120));
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						_map.tileServerCustomUrl = EditorGUILayout.TextField (_map.tileServerCustomUrl);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.HelpBox ("Use:\n$N$ for random [a-c] node (optional)\n$Z$ for zoom level (required)\n$X$ and $Y$ for X/Y tile indices (required).", MessageType.Info);
					} else if (_map.tileServer == TILE_SERVER.AerisWeather) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("Copyright Notice", GUILayout.Width (120));
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.SelectableLabel (_map.tileServerCopyrightNotice);

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("Client Id", "The client id of your Aeris Weather account."), GUILayout.Width (120));
						if (Application.isPlaying) {
							EditorGUILayout.LabelField ("**************");
						} else {
							_map.tileServerClientId = EditorGUILayout.TextField (_map.tileServerClientId);
						}
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("Secret Key", "Secret key linked to your Aeris Weather account."), GUILayout.Width (120));
						if (Application.isPlaying) {
							EditorGUILayout.LabelField ("**************");
						} else {
							_map.tileServerAPIKey = EditorGUILayout.TextField (_map.tileServerAPIKey);
						}
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("Layer Types", "Enter the desired layer types (eg: radar,radar-2m,fradar,satellite-visible,satellite,satellite-infrared-color,satellite-water-vapor,fsatellite"), GUILayout.Width (120));
						_map.tileServerLayerTypes = EditorGUILayout.TextField (_map.tileServerLayerTypes);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("Time Offset", "Enter the map time offset from now (eg. current or -10min or +1hour) or an exact date with format: YYYYMMDDhhiiss."), GUILayout.Width (120));
						_map.tileServerTimeOffset = EditorGUILayout.TextField (_map.tileServerTimeOffset);
						EditorGUILayout.EndHorizontal ();
					} else {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("Copyright Notice", GUILayout.Width (120));
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.SelectableLabel (_map.tileServerCopyrightNotice);

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("API Key", "Custom portion added to tile request url. For example: apikey=1234589"), GUILayout.Width (120));
						_map.tileServerAPIKey = EditorGUILayout.TextField (_map.tileServerAPIKey);
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Tile Resolution", "A value of 2 provides de best quality whereas a lower value will reduce downloads."), GUILayout.Width (120));
					_map.tileResolutionFactor = EditorGUILayout.Slider (_map.tileResolutionFactor, 1f, 2f);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Max Zoom Level", "Allowed maximum zoom level. Also check Zoom Distance Min under Interaction section."), GUILayout.Width (120));
					_map.tileMaxZoomLevel = EditorGUILayout.IntSlider (_map.tileMaxZoomLevel, WorldMapGlobe.TILE_MIN_ZOOM_LEVEL, WorldMapGlobe.TILE_MAX_ZOOM_LEVEL);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Transparent Tiles", "Enable this option to render tiles with transparency (slower)."), GUILayout.Width (120));
					_map.tileTransparentLayer = EditorGUILayout.Toggle (_map.tileTransparentLayer);
					EditorGUILayout.EndHorizontal ();

					if (_map.tileTransparentLayer) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Max Alpha", "Maximum level of opacity."), GUILayout.Width (120));
						_map.tileMaxAlpha = EditorGUILayout.Slider (_map.tileMaxAlpha, 0, 1f);
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Max Concurrent Downloads", "Maximum number of web downloads at any given time."), GUILayout.Width (120));
					_map.tileMaxConcurrentDownloads = EditorGUILayout.IntField (_map.tileMaxConcurrentDownloads);
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Max Loads Per Frame", "Maximum number of tiles showing up per frame."), GUILayout.Width (120));
					_map.tileMaxTileLoadsPerFrame = EditorGUILayout.IntField (_map.tileMaxTileLoadsPerFrame);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Preload Main Tiles", "Enable this option to quickly load from local cache all tiles belonging to first zoom level (Local cache must be enabled)."), GUILayout.Width (120));
					_map.tilePreloadTiles = EditorGUILayout.Toggle (_map.tilePreloadTiles);
					EditorGUILayout.EndHorizontal ();
				
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("Tiles Keep Alive", "Time in seconds to keep an inactive/hidden tile in memory before releasing it."), GUILayout.Width (120));
					_map.tileKeepAlive = EditorGUILayout.FloatField (_map.tileKeepAlive);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Show Console Errors", GUILayout.Width (120));
					_map.tileDebugErrors = EditorGUILayout.Toggle (_map.tileDebugErrors);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Enable Local Cache", GUILayout.Width (120));
					_map.tileEnableLocalCache = EditorGUILayout.Toggle (_map.tileEnableLocalCache);
					EditorGUILayout.EndHorizontal ();

					if (_map.tileEnableLocalCache) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Cache Size (Mb)", GUILayout.Width (120));
						_map.tileMaxLocalCacheSize = EditorGUILayout.LongField (_map.tileMaxLocalCacheSize, GUILayout.Width (60));
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Cache Usage", GUILayout.Width (120));
					if (tileSizeComputed) {
						GUILayout.Label ((_map.tileCurrentCacheUsage / (1024f * 1024f)).ToString ("F1") + " Mb");
					}
					if (GUILayout.Button ("Recalculate")) {
						_map.TileRecalculateCacheUsage ();
						tileSizeComputed = true;
						GUIUtility.ExitGUI ();
					}
					if (GUILayout.Button ("Purge")) {
						_map.PurgeTileCache ();
					}
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Enable Offline Tiles", GUILayout.Width (120));
					_map.tileEnableOfflineTiles = EditorGUILayout.Toggle (_map.tileEnableOfflineTiles);
					EditorGUILayout.EndHorizontal ();
					if (_map.tileEnableOfflineTiles) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Resources Path", GUILayout.Width (120));
						_map.tileResourcePathBase = EditorGUILayout.TextField (_map.tileResourcePathBase);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Only Offline Tiles", "If enabled, only existing tiles from Resources path will be loaded - cache and online tiles will be ignored."), GUILayout.Width (120));
						_map.tileOfflineTilesOnly = EditorGUILayout.Toggle (_map.tileOfflineTilesOnly);
						EditorGUILayout.EndHorizontal ();
						if (_map.tileEnableOfflineTiles) {
							EditorGUILayout.BeginHorizontal ();
							GUILayout.Label (new GUIContent ("   Fallback Texture", "Fallback texture if the tile is not found in Resources path."), GUILayout.Width (120));
							_map.tileResourceFallbackTexture = (Texture2D)EditorGUILayout.ObjectField (_map.tileResourceFallbackTexture, typeof(Texture2D), false);
							EditorGUILayout.EndHorizontal ();
						}

						if (GUILayout.Button ("Open Tiles Downloader")) {
							WorldMapTilesDownloader.ShowWindow ();
						}
					}

				} else {
					EditorGUILayout.EndHorizontal ();
				}

			}
			
			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandCountriesSection = EditorGUILayout.Foldout (expandCountriesSection, "Countries Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandCountriesSection) {

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Frontiers Detail", GUILayout.Width (120));
				_map.frontiersDetail = (FRONTIERS_DETAIL)EditorGUILayout.Popup ((int)_map.frontiersDetail, frontiersDetailOptions);
				GUILayout.Label (_map.countries.Length.ToString ());
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Inland Frontiers", GUILayout.Width (120));
				_map.showInlandFrontiers = EditorGUILayout.Toggle (_map.showInlandFrontiers);
				EditorGUILayout.EndHorizontal ();
				if (_map.showInlandFrontiers) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Color", GUILayout.Width (120));
					_map.inlandFrontiersColor = EditorGUILayout.ColorField (_map.inlandFrontiersColor);
					EditorGUILayout.EndHorizontal ();
				}
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Countries", GUILayout.Width (120));
				_map.showFrontiers = EditorGUILayout.Toggle (_map.showFrontiers);
				EditorGUILayout.EndHorizontal ();
				if (_map.showFrontiers) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Frontiers Color", GUILayout.Width (120));
					float prevAlpha = _map.frontiersColor.a;
					_map.frontiersColor = EditorGUILayout.ColorField (_map.frontiersColor);
					EditorGUILayout.EndHorizontal ();
					GUICheckTransparentColor (_map.frontiersColor.a, prevAlpha);
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Coastal Frontiers", GUILayout.Width (120));
					_map.showCoastalFrontiers = EditorGUILayout.Toggle (_map.showCoastalFrontiers);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("Line Thickness", GUILayout.Width (120));
					_map.frontiersThicknessMode = (FRONTIERS_THICKNESS)EditorGUILayout.EnumPopup (_map.frontiersThicknessMode);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					if (_map.frontiersThicknessMode == FRONTIERS_THICKNESS.Thin)
						GUI.enabled = false;
					GUILayout.Label ("Line Width", GUILayout.Width (120));
					_map.frontiersThickness = EditorGUILayout.FloatField (_map.frontiersThickness, GUILayout.Width (60));
					GUI.enabled = true;
					if (GUILayout.Button ("?", GUILayout.Width (20))) {
						EditorUtility.DisplayDialog ("Custom Width", "Please note that this option is only available on systems compatible with Shader Model 4+. Where not possible, a normal thin line will be drawn instead (many mobile devices still do not support geometry shaders)", "Ok");
					}
					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Country Highlight", GUILayout.Width (120));
				_map.enableCountryHighlight = EditorGUILayout.Toggle (_map.enableCountryHighlight);
				EditorGUILayout.EndHorizontal ();
			
				if (_map.enableCountryHighlight) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Highlight Color", GUILayout.Width (120));
					_map.fillColor = EditorGUILayout.ColorField (_map.fillColor);
					EditorGUILayout.EndHorizontal ();
				
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Draw Outline", GUILayout.Width (120));
					_map.showOutline = EditorGUILayout.Toggle (_map.showOutline);
					EditorGUILayout.EndHorizontal ();
					if (_map.showOutline) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Outline Color", GUILayout.Width (120));
						_map.outlineColor = EditorGUILayout.ColorField (_map.outlineColor);
						EditorGUILayout.EndHorizontal ();
						if (_map.surfacesCount > 75) {
							EditorGUILayout.BeginHorizontal ();
							GUIStyle warningLabelStyle = new GUIStyle (GUI.skin.label);
							warningLabelStyle.normal.textColor = new Color (0.31f, 0.38f, 0.56f);
							GUILayout.Label ("Consider disabling outline to improve performance", warningLabelStyle);
							EditorGUILayout.EndHorizontal ();
						}
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Edge Color", GUILayout.Width (120));
						_map.outlineEdgeReliefColor = EditorGUILayout.ColorField (_map.outlineEdgeReliefColor);
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Max Screen Size", "Defines the maximum screen area of a highlighted country. To prevent filling the whole screen with the highlight color, you can reduce this value and if the highlighted screen area size is greater than this factor (1=whole screen) the country won't be filled at all (it will behave as selected though)"), GUILayout.Width (120));
					_map.countryHighlightMaxScreenAreaSize = EditorGUILayout.Slider (_map.countryHighlightMaxScreenAreaSize, 0, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Include All Regions", GUILayout.Width (120));
					_map.highlightAllCountryRegions = EditorGUILayout.Toggle (_map.highlightAllCountryRegions);
					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Enable Enclaves", "Allow a country to be surrounded by another country."), GUILayout.Width (120));
				_map.enableCountryEnclaves = EditorGUILayout.Toggle (_map.enableCountryEnclaves);
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.EndVertical (); 

				EditorGUILayout.Separator ();
				EditorGUILayout.BeginVertical ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Country Names", GUILayout.Width (120));
				_map.showCountryNames = EditorGUILayout.Toggle (_map.showCountryNames);
				EditorGUILayout.EndHorizontal ();
			
				if (_map.showCountryNames) {

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Render Method", GUILayout.Width (120));
					_map.labelsRenderMethod = (LABELS_RENDER_METHOD)EditorGUILayout.Popup ((int)_map.labelsRenderMethod, labelsRenderMethods);
					EditorGUILayout.EndHorizontal ();

					if (_map.labelsRenderMethod == LABELS_RENDER_METHOD.Blended) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("  Texture Resolution", GUILayout.Width (120));
						_map.labelsQuality = (LABELS_QUALITY)EditorGUILayout.Popup ((int)_map.labelsQuality, labelsQualityOptions);
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Relative Size", GUILayout.Width (120));
					_map.countryLabelsSize = EditorGUILayout.Slider (_map.countryLabelsSize, 0.1f, 0.9f);
					EditorGUILayout.EndHorizontal ();
				
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Minimum Size", GUILayout.Width (120));
					_map.countryLabelsAbsoluteMinimumSize = EditorGUILayout.Slider (_map.countryLabelsAbsoluteMinimumSize, 0.29f, 2.5f);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Font", GUILayout.Width (120));
					_map.countryLabelsFont = (Font)EditorGUILayout.ObjectField (_map.countryLabelsFont, typeof(Font), false);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Elevation", GUILayout.Width (120));
					_map.labelsElevation = EditorGUILayout.Slider (_map.labelsElevation, 0.0f, 1.0f);
					EditorGUILayout.EndHorizontal ();
				
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Labels Color", GUILayout.Width (120));
					_map.countryLabelsColor = EditorGUILayout.ColorField (_map.countryLabelsColor);
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Draw Shadow", GUILayout.Width (120));
					_map.showLabelsShadow = EditorGUILayout.Toggle (_map.showLabelsShadow);
					EditorGUILayout.EndHorizontal ();
					if (_map.showLabelsShadow) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Shadow Color", GUILayout.Width (120));
						_map.countryLabelsShadowColor = EditorGUILayout.ColorField (_map.countryLabelsShadowColor);
						EditorGUILayout.EndHorizontal ();
					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Auto Fade Labels", GUILayout.Width (120));
					_map.countryLabelsEnableAutomaticFade = EditorGUILayout.Toggle (_map.countryLabelsEnableAutomaticFade);
					EditorGUILayout.EndHorizontal ();

					if (_map.countryLabelsEnableAutomaticFade) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("    Min Height", GUILayout.Width (120));
						_map.countryLabelsAutoFadeMinHeight = EditorGUILayout.Slider (_map.countryLabelsAutoFadeMinHeight, 0.01f, 0.25f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("    Min Height Fall Off", GUILayout.Width (120));
						_map.countryLabelsAutoFadeMinHeightFallOff = EditorGUILayout.Slider (_map.countryLabelsAutoFadeMinHeightFallOff, 0.001f, _map.countryLabelsAutoFadeMinHeight);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("    Max Height", GUILayout.Width (120));
						_map.countryLabelsAutoFadeMaxHeight = EditorGUILayout.Slider (_map.countryLabelsAutoFadeMaxHeight, 0.1f, 1.0f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("    Max Height Fall Off", GUILayout.Width (120));
						_map.countryLabelsAutoFadeMaxHeightFallOff = EditorGUILayout.Slider (_map.countryLabelsAutoFadeMaxHeightFallOff, 0.01f, 1f);
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("    Labels Per Frame", GUILayout.Width (120));
						_map.countryLabelsFadePerFrame = EditorGUILayout.IntSlider (_map.countryLabelsFadePerFrame, 1, _map.countries.Length);
						EditorGUILayout.EndHorizontal ();

					}

					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("  Upright Labels", GUILayout.Width (120));
					_map.labelsFaceToCamera = EditorGUILayout.Toggle (_map.labelsFaceToCamera);
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandProvincesSection = EditorGUILayout.Foldout (expandProvincesSection, "Provinces Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandProvincesSection) {

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Provinces", GUILayout.Width (120));
				_map.showProvinces = EditorGUILayout.Toggle (_map.showProvinces);
				EditorGUILayout.EndHorizontal ();
				if (_map.showProvinces) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Draw All Provinces", GUILayout.Width (120));
					_map.drawAllProvinces = EditorGUILayout.Toggle (_map.drawAllProvinces, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
				}
			
				if (_map.showProvinces) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Provinces Color", GUILayout.Width (120));
					float prevAlpha = _map.provincesColor.a;
					_map.provincesColor = EditorGUILayout.ColorField (_map.provincesColor);
					EditorGUILayout.EndHorizontal ();
					GUICheckTransparentColor (_map.provincesColor.a, prevAlpha);
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Enable Highlight", GUILayout.Width (120));
					_map.enableProvinceHighlight = EditorGUILayout.Toggle (_map.enableProvinceHighlight);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("      Color", GUILayout.Width (120));
					_map.provincesFillColor = EditorGUILayout.ColorField (_map.provincesFillColor);
					EditorGUILayout.EndHorizontal ();
				}
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Enable Enclaves", "Allow a province to be surrounded by another province."), GUILayout.Width (120));
				_map.enableProvinceEnclaves = EditorGUILayout.Toggle (_map.enableProvinceEnclaves);
				EditorGUILayout.EndHorizontal ();

			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandCitiesSection = EditorGUILayout.Foldout (expandCitiesSection, "Cities Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandCitiesSection) {
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Cities", GUILayout.Width (120));
				_map.showCities = EditorGUILayout.Toggle (_map.showCities);
				EditorGUILayout.EndHorizontal ();
				
				if (_map.showCities && _map.cities != null) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Cities Color", GUILayout.Width (120));
					_map.citiesColor = EditorGUILayout.ColorField (_map.citiesColor);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Region Cap. Color", GUILayout.Width (120));
					_map.citiesRegionCapitalColor = EditorGUILayout.ColorField (_map.citiesRegionCapitalColor);
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Capital Color", GUILayout.Width (120));
					_map.citiesCountryCapitalColor = EditorGUILayout.ColorField (_map.citiesCountryCapitalColor);
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Icon Size", GUILayout.Width (120));
					_map.cityIconSize = EditorGUILayout.Slider (_map.cityIconSize, 0.02f, 0.5f);
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Min Population (K)", GUILayout.Width (120));
					_map.minPopulation = EditorGUILayout.IntSlider (_map.minPopulation, 0, 3000);
					GUILayout.Label (_map.numCitiesDrawn + "/" + _map.cities.Count);
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Always Visible:", GUILayout.Width (120));
					int cityClassFilter = 0;
					bool cityBit;
					cityBit = EditorGUILayout.Toggle ((_map.cityClassAlwaysShow & WorldMapGlobe.CITY_CLASS_FILTER_REGION_CAPITAL_CITY) != 0, GUILayout.Width (20));
					GUILayout.Label ("Region Capitals");
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("", GUILayout.Width (120));
					if (cityBit)
						cityClassFilter += WorldMapGlobe.CITY_CLASS_FILTER_REGION_CAPITAL_CITY;
					cityBit = EditorGUILayout.Toggle ((_map.cityClassAlwaysShow & WorldMapGlobe.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY) != 0, GUILayout.Width (20));
					GUILayout.Label ("Country Capitals");
					if (cityBit)
						cityClassFilter += WorldMapGlobe.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY;
					_map.cityClassAlwaysShow = cityClassFilter;
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Combine Meshes", GUILayout.Width (120));
					_map.combineCityMeshes = EditorGUILayout.Toggle (_map.combineCityMeshes, GUILayout.Width (20));
					EditorGUILayout.EndHorizontal ();
					
				}
			}
			
			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			
			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandFoWSection = EditorGUILayout.Foldout (expandFoWSection, "Fog Of War Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandFoWSection) {
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Fog Of War", GUILayout.Width (120));
				_map.showFogOfWar = EditorGUILayout.Toggle (_map.showFogOfWar);
				EditorGUILayout.EndHorizontal ();
				
				if (_map.showFogOfWar) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Resolution", GUILayout.Width (120));
					_map.fogOfWarResolution = EditorGUILayout.IntSlider (_map.fogOfWarResolution, 8, 12);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Color 1", GUILayout.Width (120));
					_map.fogOfWarColor1 = EditorGUILayout.ColorField (_map.fogOfWarColor1);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Color 2", GUILayout.Width (120));
					_map.fogOfWarColor2 = EditorGUILayout.ColorField (_map.fogOfWarColor2);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Alpha", GUILayout.Width (120));
					_map.fogOfWarAlpha = EditorGUILayout.Slider (_map.fogOfWarAlpha, 0, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Elevation", GUILayout.Width (120));
					_map.fogOfWarElevation = EditorGUILayout.Slider (_map.fogOfWarElevation, 0f, 0.25f);
					EditorGUILayout.EndHorizontal ();
				}
			}

			EditorGUILayout.EndVertical (); 

			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandInteractionSection = EditorGUILayout.Foldout (expandInteractionSection, "Interaction Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandInteractionSection) {
				EditorGUILayout.BeginHorizontal ();
				if (_map.syncTimeOfDay)
					GUI.enabled = false;
				GUILayout.Label ("AutoRotation Speed", GUILayout.Width (120));
				_map.autoRotationSpeed = EditorGUILayout.Slider (_map.autoRotationSpeed, -2f, 2f);
				if (GUILayout.Button ("Stop")) {
					_map.autoRotationSpeed = 0;
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Show Cursor", GUILayout.Width (120));
				_map.showCursor = EditorGUILayout.Toggle (_map.showCursor);
				EditorGUILayout.EndHorizontal ();
			
				if (_map.showCursor) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Cursor Color", GUILayout.Width (120));
					_map.cursorColor = EditorGUILayout.ColorField (_map.cursorColor);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Follow Mouse", GUILayout.Width (120));
					_map.cursorFollowMouse = EditorGUILayout.Toggle (_map.cursorFollowMouse);
					EditorGUILayout.EndHorizontal ();
					if (Application.isPlaying) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("   Current Pos", GUILayout.Width (120));
						if (_map.mouseIsOver) {
							EditorGUILayout.Vector3Field ("", _map.cursorLocation);
						} else {
							GUILayout.Label ("(Cursor not on globe)");
						}
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Always Visible", GUILayout.Width (120));
					_map.cursorAlwaysVisible = EditorGUILayout.Toggle (_map.cursorAlwaysVisible, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Respect Other UI", GUILayout.Width (120));
				_map.respectOtherUI = EditorGUILayout.Toggle (_map.respectOtherUI);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Allow User Rotation", GUILayout.Width (120));
				_map.allowUserRotation = EditorGUILayout.Toggle (_map.allowUserRotation, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();
				if (_map.allowUserRotation) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Mode", GUILayout.Width (120));
					if (_map.syncTimeOfDay) {
						GUILayout.Label ("Set to Camera Rotates");
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("", GUILayout.Width (120));
						GUILayout.Label ("(Sync Time of Day is ON)");
					} else {
						_map.navigationMode = (NAVIGATION_MODE)EditorGUILayout.IntPopup ((int)_map.navigationMode, navigationModeOptions, navigationModeValues);
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Speed", GUILayout.Width (120));
					_map.mouseDragSensitivity = EditorGUILayout.Slider (_map.mouseDragSensitivity, 0.1f, 3);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Allowed Axis", GUILayout.Width (120));
					_map.rotationAxisAllowed = (ROTATION_AXIS_ALLOWED)EditorGUILayout.IntPopup ((int)_map.rotationAxisAllowed, rotationAxisOptions, rotationAxisValues);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Right Click Centers", GUILayout.Width (120));
					_map.centerOnRightClick = EditorGUILayout.Toggle (_map.centerOnRightClick, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Constant Drag Speed", GUILayout.Width (120));
					_map.dragConstantSpeed = EditorGUILayout.Toggle (_map.dragConstantSpeed, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					if (!_map.dragConstantSpeed) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent("   Damping Duration", "The duration of the damping rotation after a drag until the Earth stops completely."), GUILayout.Width (120));
						_map.dragDampingDuration = EditorGUILayout.FloatField (_map.dragDampingDuration);
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Drag Threshold", "Enter a threshold value to avoid accidental map dragging when clicking on HiDpi screens. Values of 5, 10, 20 or more, depending on the sensitivity of the screen."), GUILayout.Width (120));
					_map.mouseDragThreshold = EditorGUILayout.IntField (_map.mouseDragThreshold, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Right Click Rotates", GUILayout.Width (120));
					_map.rightClickRotates = EditorGUILayout.Toggle (_map.rightClickRotates, GUILayout.Width (40));
					GUI.enabled = _map.rightClickRotates;
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Clockwise Rotation", GUILayout.Width (120));
					_map.rightClickRotatingClockwise = EditorGUILayout.Toggle (_map.rightClickRotatingClockwise, GUILayout.Width (50));
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Allow Keys (WASD)", GUILayout.Width (120));
					_map.allowUserKeys = EditorGUILayout.Toggle (_map.allowUserKeys, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Keep Straight", GUILayout.Width (120));
					_map.keepStraight = EditorGUILayout.Toggle (_map.keepStraight, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Constraint Rotation", GUILayout.Width (120));
					_map.constraintPositionEnabled = EditorGUILayout.Toggle (_map.constraintPositionEnabled, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
					if (_map.constraintPositionEnabled) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Sphere Position", GUILayout.Width (120));
						_map.constraintPosition = EditorGUILayout.Vector3Field ("", _map.constraintPosition);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label ("      Max Angle", GUILayout.Width (120));
						_map.constraintAngle = EditorGUILayout.FloatField (_map.constraintAngle);
						EditorGUILayout.EndHorizontal ();
					}
				}
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Allow User Zoom", GUILayout.Width (120));
				_map.allowUserZoom = EditorGUILayout.Toggle (_map.allowUserZoom, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();
				if (_map.allowUserZoom) {
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Speed", GUILayout.Width (120));
					_map.mouseWheelSensitivity = EditorGUILayout.Slider (_map.mouseWheelSensitivity, 0.1f, 3);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label ("   Invert Direction", GUILayout.Width (120));
					_map.invertZoomDirection = EditorGUILayout.Toggle (_map.invertZoomDirection, GUILayout.Width (40));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Distance Min", "0 = default min distance"), GUILayout.Width (120));
					_map.zoomMinDistance = EditorGUILayout.FloatField (_map.zoomMinDistance, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Distance Max", "10m = default max distance"), GUILayout.Width (120));
					_map.zoomMaxDistance = EditorGUILayout.FloatField (_map.zoomMaxDistance, GUILayout.Width (50));
					EditorGUILayout.EndHorizontal ();
					if (!Application.isPlaying) {
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Label (new GUIContent ("   Zoom Level", "Set the zoom level according to the distance min/max."), GUILayout.Width (120));
						float prevZoomLevel = zoomLevel;
						zoomLevel = EditorGUILayout.Slider (zoomLevel, 0, 5f);
						if (zoomLevel != prevZoomLevel) {
							_map.SetZoomLevel (zoomLevel);
						}
						EditorGUILayout.EndHorizontal ();
					}
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Damping", "Speed for the deceleration when user stop applying zoom."), GUILayout.Width (120));
					_map.zoomDamping = EditorGUILayout.Slider (_map.zoomDamping, 0, 1f);
					EditorGUILayout.EndHorizontal ();
					if (_map.earthInvertedMode)
						GUI.enabled = false;
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Zoom Tilt", "Apply inclination when zooming in."), GUILayout.Width (120));
					_map.tilt = EditorGUILayout.Slider (_map.tilt, 0, 1f);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("      Keep Centered", "Keep flying destination centered on screen even when zoom tilt is used."), GUILayout.Width (120));
					_map.tiltKeepCentered = EditorGUILayout.Toggle (_map.tiltKeepCentered);
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Label (new GUIContent ("   Mode", "If inverted mode is enabled, zoom works by changing the field of view (no Earth nor camera moves)"), GUILayout.Width (120));
					_map.zoomMode = (ZOOM_MODE)EditorGUILayout.IntPopup ((int)_map.zoomMode, zoomModeOptions, zoomModeValues);
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal ();
				}
			
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Navigation Time", GUILayout.Width (120));
				_map.navigationTime = EditorGUILayout.Slider (_map.navigationTime, 0, 10);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Navigation Bounce", GUILayout.Width (120));
				_map.navigationBounceIntensity = EditorGUILayout.Slider (_map.navigationBounceIntensity, 0, 1);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical ();

			EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
			expandDevicesSection = EditorGUILayout.Foldout (expandDevicesSection, "Other Settings", sectionHeaderNormalStyle); 
			EditorGUILayout.EndHorizontal ();
			if (expandDevicesSection) {

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Follow Device GPS", "If set to true, the location service will be initialized (if needed) and map will be centered on current coordinates returned by the device GPS (if available)."), GUILayout.Width (120));
				_map.followDeviceGPS = EditorGUILayout.Toggle (_map.followDeviceGPS, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("VR Enabled", "Set this to true if you wish to enable interaction in normal mode. When inverted mode is enabled, VR Enabled is automatically enabled as well."), GUILayout.Width (120));
				_map.VREnabled = EditorGUILayout.Toggle (_map.VREnabled, GUILayout.Width (40));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Main Camera", "Set the camera used as main. By default it uses any camera tagged with MainCamera tag (Camera.main)"), GUILayout.Width (120));
				_map.mainCamera = (Camera)EditorGUILayout.ObjectField (_map.mainCamera, typeof(Camera), true);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Overlay Layer", "Layer index for the overlay child which is the root for all the labels and markers. Can be used to apply selective bloom effects using Beautify."), GUILayout.Width (120));
				_map.overlayLayerIndex = EditorGUILayout.LayerField (_map.overlayLayerIndex);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label (new GUIContent ("Geodata Folder Name", "Name for the geodata folder where geographic files reside. This value is used by this instance of the globe. If you have multiple scenes, each globe can point to a different geodata folder."), GUILayout.Width (120));
				_map.geodataFolderName = EditorGUILayout.TextField (_map.geodataFolderName);
				EditorGUILayout.EndHorizontal ();

			}

			EditorGUILayout.EndVertical ();

			// Extra components opener
			EditorGUILayout.Separator ();
			float buttonWidth = EditorGUIUtility.currentViewWidth * 0.4f;
			
			if (_map.gameObject.activeInHierarchy) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				
				if (extracomp [CALCULATOR]) {
					if (GUILayout.Button ("Close Calculator", GUILayout.Width (buttonWidth))) {
						WorldMapCalculator e = _map.gameObject.GetComponent<WorldMapCalculator> ();
						if (e != null)
							DestroyImmediate (e);
						UpdateExtraComponentStatus ();
						EditorGUIUtility.ExitGUI ();
					}
				} else {
					if (GUILayout.Button ("Open Calculator", GUILayout.Width (buttonWidth))) {
						WorldMapCalculator e = _map.gameObject.GetComponent<WorldMapCalculator> ();
						if (e == null)
							_map.gameObject.AddComponent<WorldMapCalculator> ();
						UpdateExtraComponentStatus ();
					}
				}
				
				if (extracomp [TICKERS]) {
					if (GUILayout.Button ("Close Ticker", GUILayout.Width (buttonWidth))) {
						WorldMapTicker e = _map.gameObject.GetComponent<WorldMapTicker> ();
						if (e != null)
							DestroyImmediate (e);
						UpdateExtraComponentStatus ();
						EditorGUIUtility.ExitGUI ();
					}
				} else {
					if (GUILayout.Button ("Open Ticker", GUILayout.Width (buttonWidth))) {
						WorldMapTicker e = _map.gameObject.GetComponent<WorldMapTicker> ();
						if (e == null)
							_map.gameObject.AddComponent<WorldMapTicker> ();
						UpdateExtraComponentStatus ();
					}
				}
				GUILayout.FlexibleSpace ();
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (extracomp [EDITOR]) {
					if (GUILayout.Button ("Close Map Editor", GUILayout.Width (buttonWidth))) {
						_map.HideProvinces ();
						_map.HideCountrySurfaces ();
						_map.HideProvinceSurfaces ();
						_map.Redraw ();
						WorldMapEditor e = _map.gameObject.GetComponent<WorldMapEditor> ();
						if (e != null) {
							WorldMapEditorInspector[] editors = (WorldMapEditorInspector[])Resources.FindObjectsOfTypeAll<WorldMapEditorInspector> ();
							if (editors.Length > 0)
								editors [0].OnCloseEditor ();
							DestroyImmediate (e);
						}
						UpdateExtraComponentStatus ();
						EditorGUIUtility.ExitGUI ();
					}
				} else {
					if (GUILayout.Button ("Open Map Editor", GUILayout.Width (buttonWidth))) {
						WorldMapEditor e = _map.gameObject.GetComponent<WorldMapEditor> ();
						if (e == null)
							_map.gameObject.AddComponent<WorldMapEditor> ();
						// cancel scenic shaders since they look awful in editor window
						if (_map.earthStyle == EARTH_STYLE.Scenic || _map.earthStyle == EARTH_STYLE.ScenicCityLights)
							_map.earthStyle = EARTH_STYLE.Natural;
						if (_map.earthStyle == EARTH_STYLE.NaturalHighResScenic || _map.earthStyle == EARTH_STYLE.NaturalHighResScenicScatter ||
						    _map.earthStyle == EARTH_STYLE.NaturalHighResScenicScatterCityLights || _map.earthStyle == EARTH_STYLE.NaturalHighResScenicCityLights ||
						    _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicScatter || _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicScatterCityLights ||
						    _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenic || _map.earthStyle == EARTH_STYLE.NaturalHighRes16KScenicCityLights)
							_map.earthStyle = EARTH_STYLE.NaturalHighRes;
						UpdateExtraComponentStatus ();
						// Unity 5.3.1 prevents raycasting in the scene view if rigidbody is present
						Rigidbody rb = _map.gameObject.GetComponent<Rigidbody> ();
						if (rb != null) {
							DestroyImmediate (rb);
							EditorGUIUtility.ExitGUI ();
							return;
						}
					}
				}
				
				if (extracomp [DECORATOR]) {
					if (GUILayout.Button ("Close Decorator", GUILayout.Width (buttonWidth))) {
						WorldMapDecorator e = _map.gameObject.GetComponent<WorldMapDecorator> ();
						if (e != null)
							DestroyImmediate (e);
						UpdateExtraComponentStatus ();
						EditorGUIUtility.ExitGUI ();
					}
				} else {
					if (GUILayout.Button ("Open Decorator", GUILayout.Width (buttonWidth))) {
						WorldMapDecorator e = _map.gameObject.GetComponent<WorldMapDecorator> ();
						if (e == null)
							_map.gameObject.AddComponent<WorldMapDecorator> ();
						UpdateExtraComponentStatus ();
					}
				}
				GUILayout.FlexibleSpace ();
				
				EditorGUILayout.EndHorizontal ();
			}
			
			EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("About", GUILayout.Width (buttonWidth * 2.0f))) {
				WorldMapAbout.ShowAboutWindow ();
			}
			GUILayout.FlexibleSpace ();
			EditorGUILayout.EndHorizontal ();
			
			if (_map.isDirty) {
				#if UNITY_5_6_OR_NEWER
				serializedObject.UpdateIfRequiredOrScript();
				#else
				serializedObject.UpdateIfDirtyOrScript ();
				#endif
				isDirty.boolValue = false;
				serializedObject.ApplyModifiedProperties ();
				EditorUtility.SetDirty (target);
			}
		}


		void GUICheckTransparentColor (float newAlpha, float prevAlpha) {
			if (newAlpha != prevAlpha)
				GUIUtility.ExitGUI ();
			if (newAlpha < 1f) {
				EditorGUILayout.HelpBox ("Transparent color selected: rendering can be slower.", MessageType.Info);
			}
		}

		
		#if !UNITY_WEBPLAYER
		// Add a menu item called "Bake Earth Texture" to a WPM's context menu.
		[MenuItem ("CONTEXT/WorldMapGlobe/Bake Earth Texture")]
		static void RestoreBackup (MenuCommand command) {
			if (!EditorUtility.DisplayDialog ("Bake Earth Texture", "This command will render the colorized areas to the current texture and save it to EarthCustom.png file inside Textures folder (existing file from a previous bake texture operation will be replaced).\n\nThis command can take some time depending on the current texture resolution and CPU speed, from a few seconds to one minute for high-res (8K) texstures.\n\nProceed?", "Ok", "Cancel"))
				return;
			
			// Proceed and restore
			string[] paths = AssetDatabase.GetAllAssetPaths ();
			string textureFolder = "";
			for (int k = 0; k < paths.Length; k++) {
				if (paths [k].EndsWith ("WorldPoliticalMapGlobeEdition/Resources/Textures")) {
					textureFolder = paths [k];
					break;
				}
			}
			if (textureFolder.Length > 0) {
				string fileName = "EarthCustom.png";
				string outputFile = textureFolder + "/" + fileName;
				if (((WorldMapGlobe)command.context).BakeTexture (outputFile) != null) {
					AssetDatabase.Refresh ();
				}
				EditorUtility.DisplayDialog ("Operation successful!", "Texture saved as \"" + fileName + "\" in WorldPoliticalMapGlobeEdition/Resources/Textures folder.\n\nTo use this texture:\n1- Check the import settings of the texture file (ensure max resolution is appropiated; should be 2048 at least, 8192 for high-res 8K textures).\n2- Set Earth style to Custom.", "Ok");
			} else {
				EditorUtility.DisplayDialog ("Required folder not found", "Cannot find \".../WorldPoliticalMapGlobeEdition/Resources/Textures\" folder!", "Ok");
			}
			
		}
		#endif


		[MenuItem ("CONTEXT/WorldMapGlobe/Tiles Downloader")]
		static void TilesDownloaderMenuOption (MenuCommand command) {
			WorldMapTilesDownloader.ShowWindow ();
		}


		
		Texture2D MakeTex (int width, int height, Color col) {
			Color[] pix = new Color[width * height];

			for (int i = 0; i < pix.Length; i++)
				pix [i] = col;

			TextureFormat tf = SystemInfo.SupportsTextureFormat (TextureFormat.RGBAFloat) ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
			Texture2D result = new Texture2D (width, height, tf, false);
			result.SetPixels (pix);
			result.Apply ();

			return result;
		}

						

	}
	
}