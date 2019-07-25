// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

//#define VR_EYE_RAY_CAST_SUPPORT  	 // Uncomment this line to support VREyeRayCast script - note that you must have already imported this script from Unity VR Samples
//#define VR_GOOGLE				  	 // Uncomment this line to support Google VR SDK (pointer and controller touch)
//#define VR_SAMSUNG_GEAR_CONTROLLER // Uncomment this line to support Samsung Gear VR SDK (laser pointer)

//#define TRACE_CTL				   // Used by us to debug/trace some events
using UnityEngine;
using UnityEngine.VR;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using WPM.Poly2Tri;
using WPM.PolygonTools;
using WPM.ClipperLib;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if VR_EYE_RAY_CAST_SUPPORT
using VRStandardAssets.Utils;
#endif
#if VR_GOOGLE
using GVR;
#endif

namespace WPM {
	[Serializable]
	[ExecuteInEditMode]
	public partial class WorldMapGlobe : MonoBehaviour {

		public const float MAP_PRECISION = 5000000f;
		public const string WPM_OVERLAY_NAME = "WPMOverlay";
		const float MIN_FIELD_OF_VIEW = 10.0f;
		const float MAX_FIELD_OF_VIEW = 85.0f;
		const float MIN_ZOOM_DISTANCE = 0.05f;
		// 0.58f;
		const float EARTH_RADIUS_KM = 6371f;
		const int SMOOTH_STRAIGHTEN_ON_POLES = -1;
		const string SPHERE_OVERLAY_LAYER_NAME = "SphereOverlayLayer";
		const string OVERLAY_MARKER_LAYER_NAME = "OverlayMarkers";
		const string MAPPER_CAM = "MapperCam";
		const String SPHERE_BACK_FACES = "WorldMapGlobeBackFaces";
		const string SURFACE_GAMEOBJECT = "Surf";

		enum OVERLAP_CLASS {
			OUTSIDE = -1,
			PARTLY_OVERLAP = 0,
			INSIDE = 1
		}

		public bool
			isDirty;
		// internal variable used to confirm changes in custom inspector - don't change its value


		#region Internal variables

		// resources
		Material outlineMat, cursorMat, gridMatOverlay, gridMatMasked;
		Material markerMatOther, markerMatLine;
		Material sphereOverlayMatDefault;
		Material fontMaterial;

		// gameObjects
		GameObject _surfacesLayer;

		GameObject surfacesLayer {
			get {
				if (_surfacesLayer == null)
					CreateSurfacesLayer ();
				return _surfacesLayer;
			}
		}

		GameObject cursorLayer, latitudeLayer, longitudeLayer;
		GameObject markersLayer, overlayMarkersLayer;

		// caché and gameObject lifetime control
		Dictionary<int, GameObject> surfaces;
		int countryProvincesDrawnIndex;
		Dictionary<Color, Material> markerMatOtherCache;
		Dictionary<Color, Material> markerMatLinesCache;
		List<TriangulationPoint> steinerPoints;
		List<Vector2> tmpPoints;

		// FlyTo functionality
		Quaternion flyToStartQuaternion, flyToEndQuaternion;
		bool flyToActive;
		float flyToStartTime, flyToDuration;
		Vector3 flyToCameraStartPosition, flyToCameraEndPosition;
		Vector3 flyToGlobeStartPosition, flyToEndDestination;
		float flyToZoomLevelStart, flyToZoomLevelEnd;
		float flyToBounceIntensity;
		bool flyToZoomLevelEnabled;
		NAVIGATION_MODE flyToMode;
		[SerializeField]
		Vector3 lockCameraPivot;
		[SerializeField]
		Quaternion lockCameraRotation;

		// UI interaction variables
		int mapUnityLayer;
		Vector3 mouseDragStart, dragDirection, mouseDragStartCursorLocation;
		bool mouseStartedDragging, hasDragged;
		float dragDampingStart;
		float wheelAccel;
		Vector3 lastRestyleEarthNormalsScaleCheck;
		bool mouseIsOverUIElement;
		int simulatedMouseButtonClick, simulatedMouseButtonPressed;
		bool leftMouseButtonClick, rightMouseButtonClick, leftMouseButtonRelease, rightMouseButtonRelease;
		bool leftMouseButtonPressed, rightMouseButtonPressed;
		float mouseDownTime;
		float lastCameraDistanceSqr, lastCameraFoV;
		float lastAtmosDistanceSqr;
		Vector3 lastAtmosGlobePosition;
		Vector3 lastCameraRotationDiff;
		Vector3 lastAtmosGlobeScale;
		RaycastHit[] hits;
		Vector3 _cursorLastLocation;
		Transform GVR_Reticle;
		bool GVR_TouchStarted;
		LineRenderer SVR_Laser;
		float lastTimeCheckVRPointers;
		float oldPinchRotationAngle = 999;

		// Overlay (Labels, tickers, ...)
		const string CURSOR_LAYER = "Cursor";
		const string LATITUDE_LINES_LAYER = "LatitudeLines";
		const string LONGITUDE_LINES_LAYER = "LongitudeLines";
		// don't change these values or
		// overlay wont' work
		public const int overlayWidth = 200;
		public const int overlayHeight = 100;
		RenderTexture overlayRT;
		GameObject overlayLayer, sphereOverlayLayer;
		Font labelsFont;
		Material labelsShadowMaterial;
		Vector3 lastSunDirection;
		Camera mapperCam;
		public bool requestMapperCamShot;
		public int currentDecoratorCount;
		Renderer backFacesRenderer;
		Material backFacesRendererMat;


		#if VR_EYE_RAY_CAST_SUPPORT
								VREyeRaycaster _VREyeRayCaster;
								VREyeRaycaster VRCameraEyeRayCaster {
								get {
				if (_VREyeRayCaster==null) {
				_VREyeRayCaster = transform.GetComponent<VREyeRaycaster>();
			}
			return _VREyeRayCaster;
		}
		}
#endif

		#endregion



		#region System initialization

		public void Init () {
			// Load materials
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": init");
#endif

			// Setup references & layers
			mapUnityLayer = gameObject.layer;

			// Updates layer in children
			foreach (Transform t in transform) {
				t.gameObject.layer = mapUnityLayer;
			}

			ReloadFont ();

			// Map materials
			frontiersMatThinOpaque = Instantiate (Resources.Load<Material> ("Materials/Frontiers"));
			frontiersMatThinOpaque.hideFlags = HideFlags.DontSave;
			frontiersMatThinAlpha = Instantiate (Resources.Load<Material> ("Materials/FrontiersAlpha"));
			frontiersMatThinAlpha.hideFlags = HideFlags.DontSave;
			frontiersMatThickOpaque = Instantiate (Resources.Load<Material> ("Materials/FrontiersGeo"));
			frontiersMatThickOpaque.hideFlags = HideFlags.DontSave;
			frontiersMatThickAlpha = Instantiate (Resources.Load<Material> ("Materials/FrontiersGeoAlpha"));
			frontiersMatThickAlpha.hideFlags = HideFlags.DontSave;
			inlandFrontiersMatOpaque = Instantiate (Resources.Load<Material> ("Materials/InlandFrontiers"));
			inlandFrontiersMatOpaque.hideFlags = HideFlags.DontSave;
			inlandFrontiersMatAlpha = Instantiate (Resources.Load<Material> ("Materials/InlandFrontiersAlpha"));
			inlandFrontiersMatAlpha.hideFlags = HideFlags.DontSave;
			hudMatCountry = Instantiate (Resources.Load<Material> ("Materials/HudCountry"));
			hudMatCountry.hideFlags = HideFlags.DontSave;
			hudMatProvince = Instantiate (Resources.Load<Material> ("Materials/HudProvince"));
			hudMatProvince.hideFlags = HideFlags.DontSave;
			hudMatProvince.renderQueue++;
			citySpot = Resources.Load<GameObject> ("Prefabs/CitySpot");
			citySpotCapitalRegion = Resources.Load<GameObject> ("Prefabs/CityCapitalRegionSpot");
			citySpotCapitalCountry = Resources.Load<GameObject> ("Prefabs/CityCapitalCountrySpot");
			citiesNormalMat = Instantiate (Resources.Load<Material> ("Materials/Cities"));
			citiesNormalMat.name = "Cities";
			citiesNormalMat.hideFlags = HideFlags.DontSave;
			citiesRegionCapitalMat = Instantiate (Resources.Load<Material> ("Materials/CitiesCapitalRegion"));
			citiesRegionCapitalMat.name = "CitiesCapitalRegion";
			citiesRegionCapitalMat.hideFlags = HideFlags.DontSave;
			citiesCountryCapitalMat = Instantiate (Resources.Load<Material> ("Materials/CitiesCapitalCountry"));
			citiesCountryCapitalMat.name = "CitiesCapitalCountry";
			citiesCountryCapitalMat.hideFlags = HideFlags.DontSave;
			provincesMatOpaque = Instantiate (Resources.Load<Material> ("Materials/Provinces"));
			provincesMatOpaque.hideFlags = HideFlags.DontSave;
			provincesMatAlpha = Instantiate (Resources.Load<Material> ("Materials/ProvincesAlpha"));
			provincesMatAlpha.hideFlags = HideFlags.DontSave;
			outlineMat = Instantiate (Resources.Load<Material> ("Materials/Outline"));
			outlineMat.name = "Outline";
			outlineMat.hideFlags = HideFlags.DontSave;
			countryColoredMat = Instantiate (Resources.Load<Material> ("Materials/CountryColorizedRegion"));
			countryColoredMat.hideFlags = HideFlags.DontSave;
			countryColoredAlphaMat = Instantiate (Resources.Load<Material> ("Materials/CountryColorizedTranspRegion"));
			countryColoredAlphaMat.hideFlags = HideFlags.DontSave;
			countryTexturizedMat = Instantiate (Resources.Load<Material> ("Materials/CountryTexturizedRegion"));
			countryTexturizedMat.hideFlags = HideFlags.DontSave;
			provinceColoredMat = Instantiate (Resources.Load<Material> ("Materials/ProvinceColorizedRegion"));
			provinceColoredMat.hideFlags = HideFlags.DontSave;
			provinceColoredAlphaMat = Instantiate (Resources.Load<Material> ("Materials/ProvinceColorizedTranspRegion"));
			provinceColoredAlphaMat.hideFlags = HideFlags.DontSave;
			provinceTexturizedMat = Instantiate (Resources.Load<Material> ("Materials/ProvinceTexturizedRegion"));
			provinceTexturizedMat.hideFlags = HideFlags.DontSave;
			cursorMat = Instantiate (Resources.Load<Material> ("Materials/Cursor"));
			cursorMat.hideFlags = HideFlags.DontSave;
			gridMatOverlay = Instantiate (Resources.Load<Material> ("Materials/GridOverlay"));
			gridMatOverlay.hideFlags = HideFlags.DontSave;
			gridMatMasked = Instantiate (Resources.Load<Material> ("Materials/GridMasked"));
			gridMatMasked.hideFlags = HideFlags.DontSave;
			markerMatOther = Instantiate (Resources.Load<Material> ("Materials/Marker"));
			markerMatOther.hideFlags = HideFlags.DontSave;
			markerMatLine = Instantiate (Resources.Load<Material> ("Materials/MarkerLine"));
			markerMatLine.hideFlags = HideFlags.DontSave;
			mountPointSpot = Resources.Load<GameObject> ("Prefabs/MountPointSpot");
			mountPointsMat = Instantiate (Resources.Load<Material> ("Materials/Mount Points"));
			mountPointsMat.hideFlags = HideFlags.DontSave;
			earthGlowMat = Instantiate (Resources.Load<Material> ("Materials/EarthGlow"));
			earthGlowMat.hideFlags = HideFlags.DontSave;
			earthGlowScatterMat = Instantiate (Resources.Load<Material> ("Materials/EarthGlow2"));
			earthGlowScatterMat.hideFlags = HideFlags.DontSave;

			countryColoredMatCache = new Dictionary<Color, Material> ();
			provinceColoredMatCache = new Dictionary<Color, Material> ();
			markerMatOtherCache = new Dictionary<Color, Material> ();
			markerMatLinesCache = new Dictionary<Color, Material> ();

			// Destroy obsolete labels layer -> now replaced with overlay feature
			GameObject o = GameObject.Find ("WPMLabels");
			if (o != null)
				DestroyImmediate (o);
			Transform tlabel = transform.Find ("LabelsLayer");
			if (tlabel != null)
				DestroyImmediate (tlabel.gameObject);
			// End destroy obsolete.

			hits = new RaycastHit[20];  // avoids GC when using RayCast

			if (_showTiles)
				InitTileSystem ();

			InitGridSystem ();

			ReloadData ();

			lastRestyleEarthNormalsScaleCheck = transform.lossyScale;

			if (gameObject.activeInHierarchy)
				StartCoroutine (CheckGPS ());

		}

		/// <summary>
		/// Reloads the data of frontiers and cities from datafiles and redraws the map.
		/// </summary>
		public void ReloadData () {
			// read baked data
			ReadCountriesPackedString ();
			if (_showProvinces) {
				ReadProvincesPackedString ();
			} else {
				_provinces = null;
			}
			if (_showCities) {
				ReadCitiesPackedString ();
			} else {
				_cities = null;
			}
			ReadMountPointsPackedString ();

			// Redraw frontiers and cities -- destroy layers if they already exists
			Redraw ();
		}

		void GetPointFromPackedString (string s, out float lat, out float lon) {
			int j = -1;
			for (int k = 0; k < s.Length; k++) {
				if (s [k] == ',') {
					j = k;
					break;
				}
			}
			if (j < 0) {
				lat = 0;
				lon = 0;
				return;
			}
			string slat = s.Substring (0, j);
			string slon = s.Substring (j + 1);
			lat = float.Parse (slat, Misc.InvariantCulture) / MAP_PRECISION;
			lon = float.Parse (slon, Misc.InvariantCulture) / MAP_PRECISION;
		}


		#endregion


		#region Game loop events


		void OnEnable () {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": enable wpm");
#endif
			if (string.IsNullOrEmpty (geodataFolderName)) {
				geodataFolderName = "Geodata";
			}
			
			if ((int)_earthStyle == 20) {	// migration to new property
				_earthStyle = EARTH_STYLE.Alternate1;
				_showTiles = true;
			}

			if (_overlayLayerIndex == 0) {
				_overlayLayerIndex = gameObject.layer;
			}

#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            PrefabInstanceStatus prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (prefabStatus != PrefabInstanceStatus.NotAPrefab) {
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
#else
			PrefabUtility.DisconnectPrefabInstance (gameObject);
#endif
#endif

			// Check backfaces
			Transform tBackFaces = transform.Find (SPHERE_BACK_FACES);
			if (tBackFaces == null) {
				GameObject backFaces = Instantiate<GameObject> (Resources.Load<GameObject> ("Prefabs/WorldMapGlobeBackFaces")) as GameObject;
				backFaces.name = SPHERE_BACK_FACES;
				backFaces.hideFlags = HideFlags.DontSave;
				backFaces.transform.SetParent (transform, false);
				backFaces.transform.localPosition = Misc.Vector3zero;
				tBackFaces = backFaces.transform;
												
			}
			backFacesRenderer = tBackFaces.GetComponent<Renderer> ();
			backFacesRendererMat = backFacesRenderer.sharedMaterial;

			if (_countries == null) {
				Init ();
			}

			// Check material
			if (earthRenderer.sharedMaterial == null) {
				RestyleEarth ();
			}

			UpdateMoon ();
			UpdateSkybox ();

			if (hudMatCountry != null && hudMatCountry.color != _fillColor) {
				hudMatCountry.color = _fillColor;
			}
			if (hudMatProvince != null && hudMatProvince.color != _provincesFillColor) {
				hudMatProvince.color = _provincesFillColor;
			}
			if (citiesNormalMat.color != _citiesColor) {
				citiesNormalMat.color = _citiesColor;
			}
			if (citiesRegionCapitalMat.color != _citiesRegionCapitalColor) {
				citiesRegionCapitalMat.color = _citiesRegionCapitalColor;
			}
			if (citiesCountryCapitalMat.color != _citiesCountryCapitalColor) {
				citiesCountryCapitalMat.color = _citiesCountryCapitalColor;
			}
			outlineMat.color = _outlineColor;
			outlineMat.SetColor ("_EdgeColor", _outlineEdgeReliefColor);
			if (cursorMat.color != _cursorColor) {
				cursorMat.color = _cursorColor;
			}
			if (gridMatOverlay.color != _gridColor) {
				gridMatOverlay.color = _gridColor;
			}
			if (gridMatMasked.color != _gridColor) {
				gridMatMasked.color = _gridColor;
			}

			if (_allowUserZoom) {
				mainCamera.nearClipPlane = Mathf.Clamp (transform.lossyScale.x / 100f, 0.01f, 0.1f);
			}
			lastCameraDistanceSqr = (mainCamera.transform.position - transform.position).sqrMagnitude;

			// Unity 5.3.1 prevents raycasting in the scene view if rigidbody is present - we have to delete it in editor time but re-enable it here during play mode
			if (Application.isPlaying) {
				Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
				if (rb == null) {
					rb = gameObject.AddComponent<Rigidbody> ();
					rb.useGravity = false;
					rb.isKinematic = true;
				}
			}

			RestoreCameraPos ();
			_mainCamera.transform.LookAt (transform.position);
			CopyCameraPos ();
		}

		void Start () {
			RegisterVRPointers ();
		}

		void RegisterVRPointers () {
			if (Time.time - lastTimeCheckVRPointers < 1f)
				return;
			lastTimeCheckVRPointers = Time.time;

#if VR_GOOGLE
												GameObject obj = GameObject.Find ("GvrControllerPointer");
												if (obj != null) {
																Transform t = obj.transform.Find ("Laser");
																if (t != null) {
																				GVR_Reticle = t.Find ("Reticle");
																}
												}
#elif VR_SAMSUNG_GEAR_CONTROLLER
//			GameObject obj = GameObject.Find ("GearVrController");
//			if (obj != null) {
//				Transform t = obj.transform.Find ("Model/Laser");
//				if (t != null) {
//					SVR_Laser = t.gameObject.GetComponent<LineRenderer>();
//				}
//			}
			if (SVR_Laser == null) {
				OVRGearVrController[] cc = FindObjectsOfType<OVRGearVrController> ();
				for (int k=0; k<cc.Length; k++) {
					{
						if (cc [k].m_model.activeSelf) {
							Transform t = cc [k].transform.Find ("Model/Laser");
						
							if (t != null) {
								SVR_Laser = t.gameObject.GetComponent<LineRenderer> ();            
							}
						}
					}
				}
			}
#endif
		}

		void OnDestroy () {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": destroy wpm");
#endif
			DestroyOverlay ();
			DestroySurfaces ();
			DestroyTiles ();
			DestroyGridSystem ();
			DestroyFogOfWarLayer ();
		}

		void OnMouseEnter () {
			mouseIsOver = true;
		}

		void OnMouseExit () {
			// Check if it's really outside of sphere
			Vector3 dummy;
			Ray dummyRay;
			if (!GetHitPoint (out dummy, out dummyRay)) {
				mouseIsOver = false;
				mouseStartedDragging = false;
				HideCountryRegionHighlight ();
				HideHighlightedCell ();
			}
		}

		void Reset () {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": reset");
#endif
			Redraw ();
		}

		void Update () {
			if (!Application.isPlaying) {
				// when saving the scene from Editor, the material of the sphere label layer is cleared - here's a fix to recreate it
				if (_showCountryNames && sphereOverlayLayer != null && sphereOverlayLayer.GetComponent<Renderer> () == null) {
					CreateOverlay ();
				}
			}

			if (_sun != null) {
				if (_syncTimeOfDay) {
					System.DateTime Now = System.DateTime.Now.ToUniversalTime ();                // Get unlocalised time
					float SolarDeclination = -23.45f * Mathf.Cos ((360f / 365f) * (Now.DayOfYear + 10) * Mathf.Deg2Rad);                                 // Get day of year, convert to a -1 to 1 value 
					float sunRot = ((Now.Hour * 60f) + Now.Minute + (Now.Second / 60f)) / 4f;     // Convert time into minutes, then scale to a 0-360 range value
					Vector3 sunRotation = new Vector3 (SolarDeclination, sunRot, 0);        // Combine the axis and calculated sun angle into a vector
					_sun.transform.localRotation = Quaternion.Euler (sunRotation);
					transform.rotation = Misc.QuaternionZero;
					_navigationMode = NAVIGATION_MODE.CAMERA_ROTATES;
					_autoRotationSpeed = 0;
				}
				_earthScenicLightDirection = -_sun.forward;
			}

			Camera cam = mainCamera;

			RestoreCameraPos ();

			if (Application.isPlaying) {
				// Check mouse buttons state
				leftMouseButtonClick = Input.GetMouseButtonDown (0) || simulatedMouseButtonClick == 1 || Input.GetButtonDown ("Fire1");
#if VR_GOOGLE
																if (GvrController.TouchDown) {
																				GVR_TouchStarted = true;
																				leftMouseButtonClick = true;
																}
#endif

				leftMouseButtonPressed = leftMouseButtonClick || Input.GetMouseButton (0) || simulatedMouseButtonPressed == 1;
#if VR_GOOGLE
                if (GVR_TouchStarted) {
																				leftMouseButtonPressed = true;
                                            mouseDownTime = Time.time;
                }
#endif

				leftMouseButtonRelease = Input.GetMouseButtonUp (0) || simulatedMouseButtonClick == 1 || Input.GetButtonUp ("Fire1");
#if VR_GOOGLE
																if (GvrController.TouchUp) {
																				GVR_TouchStarted = false;
																				leftMouseButtonRelease = true;
																}
#endif

				rightMouseButtonClick = Input.GetMouseButtonDown (1) || simulatedMouseButtonClick == 2;
				rightMouseButtonPressed = rightMouseButtonClick || Input.GetMouseButton (1) || simulatedMouseButtonPressed == 2;
				rightMouseButtonRelease = Input.GetMouseButtonUp (1) || simulatedMouseButtonClick == 2;
				if (Input.GetMouseButtonDown (0))
					mouseDownTime = Time.time;

				// Check if navigateTo... has been called and in this case rotate the globe until the country is centered
				if (flyToActive) {
					RotateToDestination ();
				} else {
					// subtle/slow continuous rotation
					if (!constraintPositionEnabled && _autoRotationSpeed != 0) {
						transform.Rotate (Misc.Vector3up, -_autoRotationSpeed);
					}
				}

				CheckCursorVisibility ();

				// Check whether the points is on an UI element, then cancels
				if (_respectOtherUI) {
#if VR_EYE_RAY_CAST_SUPPORT
				if (VRCameraEyeRayCaster!=null && VRCameraEyeRayCaster.CurrentInteractible != null) {
					if (!mouseIsOverUIElement) {
						mouseIsOverUIElement = true;
						HideCountryRegionHighlight();
					}
					CheckTilt();
					return;
				}
#endif
					if (UnityEngine.EventSystems.EventSystem.current != null) {
						if ((Input.touchSupported && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)) || // mobile
						    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (-1)) {   // non-mobile
							if (!mouseIsOverUIElement) {
								mouseIsOverUIElement = true;
							}
							CheckTilt ();
							return;
						}
					}
				}
				mouseIsOverUIElement = false;

				// Handle interaction mode
				if (_earthInvertedMode) {
					CheckUserInteractionInvertedMode ();
				} else {
					CheckUserInteractionNormalMode ();
				}
			}

			// Zoom Tilt?
			CheckTilt ();

			// Has moved?
			Vector3 cameraRotationDiff = (cam.transform.eulerAngles - transform.eulerAngles);
			float cameraDistanceSqr = FastVector.SqrDistanceByValue (cam.transform.position, transform.position);
			// Fades country labels and updates borders
			if (cameraRotationDiff != lastCameraRotationDiff || cameraDistanceSqr != lastCameraDistanceSqr || cam.fieldOfView != lastCameraFoV) {

				lastCameraDistanceSqr = cameraDistanceSqr;
				lastCameraRotationDiff = cameraRotationDiff;
				lastCameraFoV = cam.fieldOfView;
				shouldCheckTiles = true;
				resortTiles = true;

				if (_countryLabelsEnableAutomaticFade && _showCountryNames) {
					FadeCountryLabels ();
				}

				// Check maximum screen area size for highlighted country
				if (_countryHighlightMaxScreenAreaSize < 1f && _countryRegionHighlighted != null && countryRegionHighlightedObj != null && countryRegionHighlightedObj.activeSelf) {
					if (!CheckScreenAreaSizeOfRegion (_countryRegionHighlighted)) {
						countryRegionHighlightedObj.SetActive (false);
					}
				}
			}

			// Verify if mouse enter a country boundary - we only check if mouse is inside the sphere of world
			if (mouseIsOver || _VREnabled) {
				if (Application.isMobilePlatform) {
					if (_VREnabled || leftMouseButtonClick) {
						CheckMousePos ();
					} else if (leftMouseButtonPressed) {
						UpdateCursorLocation ();
					}
				} else {
					CheckMousePos ();
				}

				// Remember the last element clicked & trigger events
				if (dragDampingStart == 0 && (leftMouseButtonClick || rightMouseButtonClick)) {
					_countryLastClicked = _countryHighlightedIndex;
					_countryRegionLastClicked = _countryRegionHighlightedIndex;
					if (_countryLastClicked >= 0 && OnCountryClick != null) {
						OnCountryClick (_countryHighlightedIndex, _countryRegionHighlightedIndex);
					}
					_provinceLastClicked = _provinceHighlightedIndex;
					_provinceRegionLastClicked = _provinceRegionHighlightedIndex;
					if (_provinceLastClicked >= 0 && OnProvinceClick != null) {
						OnProvinceClick (_provinceLastClicked, _provinceRegionLastClicked);
					}
					_cityLastClicked = _cityHighlightedIndex;
					if (_cityLastClicked >= 0 && OnCityClick != null) {
						OnCityClick (_cityLastClicked);
					}
					if (OnMouseDown != null) {
						OnMouseDown (_cursorLocation, leftMouseButtonClick ? 0 : 1);
					}
				} else {
					if ((leftMouseButtonRelease || rightMouseButtonRelease) && OnClick != null && Time.time - mouseDownTime < 0.5f && !CheckDragThreshold (mouseDragStart, Input.mousePosition, 10)) {
						OnClick (_cursorLocation, leftMouseButtonRelease ? 0 : 1);
					}
				}
				if (leftMouseButtonPressed && _cursorLastLocation != _cursorLocation && OnDrag != null) {
					OnDrag (_cursorLocation);
				}
				_cursorLastLocation = _cursorLocation;
			}

			if (leftMouseButtonRelease && OnMouseRelease != null)
				OnMouseRelease (_cursorLocation, 0);
			if (rightMouseButtonRelease && OnMouseRelease != null)
				OnMouseRelease (_cursorLocation, 1);

			// Reset simulated click
			simulatedMouseButtonClick = 0;
			simulatedMouseButtonPressed = 0;
		}

		void CheckTilt () {
			CopyCameraPos ();
			if (_tilt > 0 && !_earthInvertedMode) {
				UpdateTiltedView ();
			}
		}

		void RestoreCameraPos () {
			if (_tilt > 0 && tiltKeepCentered) {
				Camera cam = mainCamera;
				if (cam != null) {
					cam.transform.position = lockCameraPivot;
					cam.transform.rotation = lockCameraRotation;
				}
			}
		}

		void CopyCameraPos () {
			lockCameraPivot = mainCamera.transform.position;
			lockCameraRotation = _mainCamera.transform.rotation;
		}

		void LateUpdate () {
			// Check mapper cam
			if (requestMapperCamShot) {
				if (mapperCam == null || (overlayRT != null && !overlayRT.IsCreated ())) {
					DestroyOverlay ();
					Redraw ();
				}
				if (mapperCam != null) {
					mapperCam.Render ();
				}
				requestMapperCamShot = false;
			}

			// Updates atmosphere if Sun light has changed direction
			if (lastAtmosDistanceSqr != lastCameraDistanceSqr || _earthScenicLightDirection != lastSunDirection || !Application.isPlaying || lastAtmosGlobePosition != transform.position || lastAtmosGlobeScale != transform.lossyScale) {
				lastSunDirection = _earthScenicLightDirection;
				lastAtmosDistanceSqr = lastCameraDistanceSqr;
				lastAtmosGlobePosition = transform.position;
				lastAtmosGlobeScale = transform.lossyScale;
				DrawAtmosphere ();
			}
			if (_showTiles) {
				LateUpdateTiles ();
			}

			if (_showHexagonalGrid) {
				UpdateHexagonalGrid ();
			}

			if (!leftMouseButtonPressed) {
				mouseStartedDragging = false;
				hasDragged = false;
			}
		}

		#endregion


		#region Drawing stuff

		/// <summary>
		/// Clears and Repaints the Globe's features (frontiers, cities, provinces, grid, ...)
		/// </summary>
		public void Redraw () {
			if (!gameObject.activeInHierarchy)
				return;

			if (countries == null)
				OnEnable ();

#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": Redraw");
#endif

			DestroyMapLabels ();

			InitSurfacesCache (); // Initialize surface cache

			HideProvinces ();

			RestyleEarth (); // Apply texture to Earth

			DrawFrontiers ();    // Redraw country frontiers

			DrawInlandFrontiers (); // Redraw inland frontiers

			DrawAllProvinceBorders (true); // Redraw province borders

			DrawCities ();       // Redraw cities layer

			DrawMountPoints ();  // Redraw mount points (only in Editor time)

			DrawCursor ();       // Draw cursor lines

			DrawGrid ();     // Draw longitude & latitude lines

			DrawAtmosphere ();

			DrawMapLabels ();

			DrawFogOfWar ();

		}

		void InitSurfacesCache () {
			if (surfaces != null) {
				List<GameObject> cached = new List<GameObject> (surfaces.Values);
				int cachedCount = cached.Count;
				for (int k = 0; k < cachedCount; k++) {
					if (cached [k] != null) {
						DestroyImmediate (cached [k]);
					}
				}
				surfaces.Clear ();
			} else {
				surfaces = new Dictionary<int, GameObject> ();
			}
			_surfacesCount = 0;
			DestroySurfacesLayer ();
		}

		void CreateSurfacesLayer () {
			Transform t = transform.Find ("Surfaces");
			if (t != null) {
				DestroyImmediate (t.gameObject);
				for (int k = 0; k < countries.Length; k++) {
					int regionsCount = countries [k].regions.Count;
					for (int r = 0; r < regionsCount; r++)
						countries [k].regions [r].customMaterial = null;
				}
			}
			_surfacesLayer = new GameObject ("Surfaces");
			_surfacesLayer.transform.SetParent (transform, false);
			_surfacesLayer.transform.localScale = _earthInvertedMode ? Misc.Vector3one * 0.995f : Misc.Vector3one;
		}

		void DestroySurfacesLayer () {
			if (_surfacesLayer != null) {
				GameObject.DestroyImmediate (_surfacesLayer);
			}
		}

		Material GetColoredMarkerOtherMaterial (Color color) {
			Material mat;
			if (markerMatOtherCache.TryGetValue (color, out mat)) {
				return mat;
			} else {
				Material customMat;
				customMat = Instantiate (markerMatOther);
				customMat.name = markerMatOther.name;
				markerMatOtherCache [color] = customMat;
				customMat.color = color;
				customMat.hideFlags = HideFlags.DontSave;
				return customMat;
			}
		}

		Material GetColoredMarkerLineMaterial (Color color) {
			Material mat;
			if (markerMatLinesCache.TryGetValue (color, out mat)) {
				return mat;
			} else {
				Material customMat;
				customMat = Instantiate (markerMatLine);
				customMat.name = markerMatLine.name;
				markerMatLinesCache [color] = customMat;
				customMat.color = color;
				customMat.hideFlags = HideFlags.DontSave;
				return customMat;
			}
		}

		void ApplyMaterialToSurface (GameObject obj, Material sharedMaterial) {
			if (obj != null) {
				Renderer[] rr = obj.GetComponentsInChildren<Renderer> (true);
				for (int k = 0; k < rr.Length; k++) {
					Renderer r = rr [k];
					if (r != null && r.name.Equals (SURFACE_GAMEOBJECT)) {
						r.sharedMaterial = sharedMaterial;
					}
				}
			}
		}

		void ToggleGlobalVisibility (bool visible) {
			Renderer[] rr = transform.GetComponentsInChildren<MeshRenderer> ();
			for (int k = 0; k < rr.Length; k++) {
				rr [k].enabled = visible;
			}
			if (overlayLayer != null) {
				Transform billboard = overlayLayer.transform.Find ("Billboard");
				if (billboard != null) {
					Renderer billboardRenderer = billboard.GetComponent<Renderer> ();
					if (billboardRenderer != null)
						billboardRenderer.enabled = false;
				}
			}
			enabled = visible;
		}

		#endregion

		#region Internal functions

		float ApplyDragThreshold (float value, float threshold) {
			if (threshold > 0) {
				if (value < 0) {
					value += threshold;
					if (value > 0)
						value = 0;
				} else {
					value -= threshold;
					if (value < 0)
						value = 0;
				}
			}
			return value;
		}

		/// <summary>
		/// Returns true if drag is detected based on displacement threshold
		/// </summary>
		bool CheckDragThreshold (Vector3 v1, Vector3 v2, float threshold) {
			if (threshold <= 0f)
				return true;

			float dx = v1.x - v2.x;
			if (dx <= -threshold || dx >= threshold) {
				return true;
			}
			float dy = v1.y - v2.y;
			if (dy <= -threshold || dy >= threshold) {
				return true;
			}
			return false;
		}


		/// <summary>
		/// Used internally to rotate the globe or the camera during FlyTo operations. Use FlyTo method.
		/// </summary>
		void RotateToDestination () {
			float delta, t;
			Camera cam = mainCamera;

			if (flyToDuration == 0) {
				delta = flyToDuration;
				t = 1;
			} else {
				delta = (Time.time - flyToStartTime);
				t = Mathf.SmoothStep (0, 1, delta / flyToDuration);
			}
			if (flyToMode == NAVIGATION_MODE.EARTH_ROTATES || _earthInvertedMode) {
				if (transform.position != flyToGlobeStartPosition) {
					flyToGlobeStartPosition = transform.position;
					flyToEndQuaternion = GetQuaternion (flyToEndDestination.normalized);
				}
				// Apply new zoom level
				if (flyToZoomLevelEnabled) {
					SetZoomLevel (LerpZoomLevel (t));
				}
				transform.rotation = Quaternion.Lerp (flyToStartQuaternion, flyToEndQuaternion, t);
			} else {
				// Apply new zoom level
				Vector3 upVector = Vector3.Lerp (cam.transform.up, transform.up, t);
				if (flyToZoomLevelEnabled) {
					SetZoomLevel (LerpZoomLevel (t));
				}
				float camDistance = (cam.transform.position - transform.position).magnitude;
				cam.transform.position = transform.position + Vector3.Lerp (flyToCameraStartPosition, flyToCameraEndPosition, t).normalized * camDistance;

				cam.transform.LookAt (transform.position, upVector);
				float t2 = Mathf.Sin (Mathf.PI * 0.5f * delta / flyToDuration);
				cam.transform.rotation = Quaternion.Lerp (flyToStartQuaternion, cam.transform.rotation, t2);
			}

			if (delta >= flyToDuration) {
				flyToActive = false;
				if (OnFlyEnd != null)
					OnFlyEnd (GetCurrentMapLocation ());
			}
		}

		float LerpZoomLevel (float t) {
			float zl = Mathf.Lerp (flyToZoomLevelStart, flyToZoomLevelEnd, t);
			if (flyToBounceIntensity > 0) {
				zl += Mathf.Sin (t * Mathf.PI) * flyToBounceIntensity;
			}
			return zl;
		}

		void UpdateTiltedView () {
			Camera cam = Camera.main;
			float a = transform.lossyScale.y * 0.5f;
			float c = Vector3.Distance (cam.transform.position, transform.position);
			float b = Mathf.Sqrt (c * c - a * a);
			float cosA = (a * a - b * b - c * c) / (-2 * b * c);
			float angle = Mathf.Acos (cosA) * Mathf.Rad2Deg;
			if (_zoomMode == ZOOM_MODE.CAMERA_MOVES) {
				if (tiltKeepCentered) {
					Vector3 lockTarget = transform.InverseTransformPoint (cam.transform.position).normalized * 0.5f;
					Vector3 wpos = transform.TransformPoint (lockTarget);
					cam.transform.LookAt (wpos, cam.transform.up);
					cam.transform.RotateAround (wpos, -cam.transform.right, angle * _tilt);
				} else {
					cam.transform.LookAt (transform.position, cam.transform.up);
					Quaternion lookRotation = Quaternion.AngleAxis (-angle * _tilt, cam.transform.right);
					cam.transform.rotation = lookRotation * cam.transform.rotation;
				}
			} else {
				transform.position = cam.transform.position + cam.transform.forward * c;
				Quaternion oldRotation = transform.rotation;
				transform.RotateAround (cam.transform.position, cam.transform.right, angle * _tilt);
				transform.rotation = oldRotation;
			}
		}

		Quaternion GetCameraStraightLookRotation () {
			Camera cam = mainCamera;
			Vector3 camVec = transform.position - cam.transform.position;
			if (Mathf.Abs (Vector3.Dot (transform.up, camVec.normalized)) > 0.96f) {   // avoid going crazy around poles
				return cam.transform.rotation;
			}

			Quaternion old = cam.transform.rotation;
			cam.transform.LookAt (transform.position);
			Vector3 camUp = Vector3.ProjectOnPlane (transform.up, camVec);
			float angle = SignedAngleBetween (cam.transform.up, camUp, camVec);
			cam.transform.Rotate (camVec, angle, Space.World);
			Quaternion q = cam.transform.rotation;
			cam.transform.rotation = old;
			return q;
		}

		/// <summary>
		/// Returns optimum distance between camera and a region maxWidth
		/// </summary>
		float GetFrustumZoomLevel (float width, float height) {
			Camera cam = mainCamera;
			if (cam == null)
				return 1;
			if (cam.orthographic)
				return 1;
			
			float fv = cam.fieldOfView;
			float radAngle = fv * Mathf.Deg2Rad;
			float aspect = cam.aspect; 
			float frustumDistanceH = height * 0.5f / Mathf.Tan (radAngle * 0.5f);
			float frustumDistanceW = (width / aspect) * 0.5f / Mathf.Tan (radAngle * 0.5f);
			float radius = transform.lossyScale.x * 0.5f;
			float frustumDistance = radius + Mathf.Max (frustumDistanceH, frustumDistanceW);

			float sphereY = radius * Mathf.Sin (radAngle);
			float sphereX = radius * Mathf.Cos (radAngle);
			float frustumDistanceSphere = sphereY / Mathf.Tan (radAngle * 0.5f) + sphereX;
			float minRadius = radius + _zoomMinDistance + MIN_ZOOM_DISTANCE;
			float zoomLevel = (frustumDistance - minRadius) / (frustumDistanceSphere - minRadius);

			return zoomLevel;

		}


		void CheckUserInteractionNormalMode () {

			float radius = transform.lossyScale.y * 0.5f;
			Camera cam = mainCamera;

			// if mouse/finger is over map, implement drag and rotation of the world
			if (mouseIsOver) {
				// Use left mouse button and drag to rotate the world
				if (_allowUserRotation) {
					if (leftMouseButtonClick) {
#if VR_GOOGLE
																								mouseDragStart = GvrController.TouchPos;
#else
						mouseDragStart = Input.mousePosition;
						if (Application.isMobilePlatform) {
							UpdateCursorLocation (); // _cursorLocation has not been set yet so we call CheckMousePos before any interaction
						}
#endif

						mouseDragStartCursorLocation = _cursorLocation;
						mouseStartedDragging = true;
						hasDragged = false;
					} else if (mouseStartedDragging && leftMouseButtonPressed && Input.touchCount < 2) {
						if (_dragConstantSpeed) {
							if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
								mouseDragStartCursorLocation.y = 0;
								_cursorLocation.y = 0;
							} else if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY) {
								mouseDragStartCursorLocation.x = 0;
								_cursorLocation.x = 0;
							}
							if (CheckDragThreshold (mouseDragStart, Input.mousePosition, _mouseDragThreshold)) {
								float angle = Vector3.Angle (mouseDragStartCursorLocation, _cursorLocation);
								if (angle != 0) {
									hasDragged = true;
									if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
										Vector3 axis = Vector3.Cross (mouseDragStartCursorLocation, _cursorLocation);
										Vector3 angles = Quaternion.AngleAxis (angle, axis).eulerAngles;
										transform.Rotate (angles);
									} else {
										Vector3 axis = Vector3.Cross (transform.TransformVector (mouseDragStartCursorLocation), transform.TransformVector (_cursorLocation));
										RotateAround (cam.transform, transform.position, axis, -angle);
									}
									mouseDragStart = Input.mousePosition;
								}
							}
						} else {
							float distFactor = Mathf.Min ((Vector3.Distance (cam.transform.position, transform.position) - radius) / radius, 1f);
#if VR_GOOGLE
																												dragDirection = (mouseDragStart - (Vector3)GvrController.TouchPos);
																												dragDirection.y *= -1.0f;
																												dragDirection.x = ApplyDragThreshold (dragDirection.x);
																												dragDirection.y = ApplyDragThreshold (dragDirection.y);
																												dragDirection *= distFactor * _mouseDragSensitivity;
#else
							dragDirection = mouseDragStart - Input.mousePosition;
							if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
								dragDirection.y = 0;
							} else if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY) {
								dragDirection.x = 0;
							}
							dragDirection.x = ApplyDragThreshold (dragDirection.x, _mouseDragThreshold);
							dragDirection.y = ApplyDragThreshold (dragDirection.y, _mouseDragThreshold);
							dragDirection *= 0.01f * distFactor * _mouseDragSensitivity;
#endif

							if (dragDirection.x != 0 || dragDirection.y != 0) {
								hasDragged = true;
								if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
									transform.Rotate (cam.transform.up, dragDirection.x, Space.World);
									Vector3 axisY = Vector3.Cross (transform.position - cam.transform.position, cam.transform.up);
									transform.Rotate (axisY, dragDirection.y, Space.World);
								} else {
									if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
										cam.transform.RotateAround (transform.position, transform.up, -dragDirection.x);
									} else {
										RotateAround (cam.transform, transform.position, cam.transform.up, -dragDirection.x);
										RotateAround (cam.transform, transform.position, cam.transform.right, dragDirection.y);
									}
								}
//								dragDamping = 1;
								dragDampingStart = Time.time;
							}
						}
						flyToActive = false;
					}
				}

				// Use right mouse button and drag to spin the world around z-axis
				if (!flyToActive) {
					float rotAngle = 0;
					if (Input.touchCount == 2) {
						// Store both touches.
						Touch touchZero = Input.GetTouch (0);
						Touch touchOne = Input.GetTouch (1);

						Vector2 v2 = touchZero.position - touchOne.position;
																								

						float newAngle = Mathf.Atan2 (v2.y, v2.x) * Mathf.Rad2Deg;
						if (oldPinchRotationAngle != 999) {
							rotAngle = Mathf.DeltaAngle (newAngle, oldPinchRotationAngle);
							if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES)
								rotAngle *= -1f;

						}


						oldPinchRotationAngle = newAngle;
					} else {
						oldPinchRotationAngle = 999;
						if (rightMouseButtonPressed) {
							if (_showProvinces && _provinceHighlightedIndex >= 0 && _centerOnRightClick && rightMouseButtonClick) {
								FlyToProvince (_provinceHighlightedIndex, 0.8f);
							} else if (_countryHighlightedIndex >= 0 && _centerOnRightClick && rightMouseButtonClick) {
								FlyToCountry (_countryHighlightedIndex, 0.8f);
							} else if (_allowUserRotation && _rightClickRotates) {
								rotAngle = _rightClickRotatingClockwise ? -2f : 2f;
							}
						}
					}
					if (rotAngle != 0) {
						Vector3 axis = (transform.position - cam.transform.position).normalized;
						if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
							transform.Rotate (axis, rotAngle, Space.World);
						} else {
							cam.transform.Rotate (axis, rotAngle, Space.World);
						}
					}
				}
			}

			// Check special keys
			if (_allowUserKeys && _allowUserRotation) {
				bool pressed = false;
				Vector3 dragKeyVert = Misc.Vector3zero;
				if (Input.GetKey (KeyCode.W)) {
					dragKeyVert = Misc.Vector3down;
					pressed = true;
				} else if (Input.GetKey (KeyCode.S)) {
					dragKeyVert = Misc.Vector3up;
					pressed = true;
				}
				Vector3 dragKeyHoriz = Misc.Vector3zero;
				if (Input.GetKey (KeyCode.A)) {
					dragKeyHoriz = Misc.Vector3right;
					pressed = true;
				} else if (Input.GetKey (KeyCode.D)) {
					dragKeyHoriz = Misc.Vector3left;
					pressed = true;
				}
				if (pressed) {
					dragDirection = dragKeyVert + dragKeyHoriz;
					float distFactor = Mathf.Min ((Vector3.Distance (cam.transform.position, transform.position) - radius) / radius, 1f);
					dragDirection *= distFactor * _mouseDragSensitivity;
					if (_dragConstantSpeed) {
						dragDirection *= 18f;
						dragDampingStart = Time.time + dragDampingDuration;
//						dragDamping = 18;
					} else {
//						dragDamping = 1;
						dragDampingStart = Time.time;
					}
				}
			}

			// Check contraint
			if (constraintPositionEnabled) {
				// Check constraint around position
				Vector3 oldCamPos = cam.transform.position;
				Vector3 globePos = transform.position;
				transform.position = Misc.Vector3zero;
				cam.transform.position -= globePos;
				Vector3 camPos = cam.transform.position;
				Ray ray = new Ray (camPos, cam.transform.forward);
				int hitCount = Physics.RaycastNonAlloc (ray, hits, Mathf.Min (lastCameraDistanceSqr, 10000), layerMask);
				if (hitCount > 0) {
					Vector3 contraintWPos = transform.TransformPoint (constraintPosition);
					for (int k = 0; k < hitCount; k++) {
						if (hits [k].collider.gameObject == gameObject) {
							Vector3 axis = Vector3.Cross (contraintWPos, hits [k].point);
							float angleDiff = SignedAngleBetween (contraintWPos, hits [k].point, axis);
							if (Mathf.Abs (angleDiff) > constraintAngle + 0.0001f) {
								if (angleDiff > 0) {
									angleDiff = constraintAngle - angleDiff;
								} else {
									angleDiff = angleDiff - constraintAngle;
								}
								if (_navigationMode == NAVIGATION_MODE.CAMERA_ROTATES) {
									axis = Vector3.Cross (contraintWPos - camPos, hits [k].point - camPos);
									Vector3 prevUp = cam.transform.up;
									cam.transform.Rotate (axis, angleDiff, Space.World);
									cam.transform.LookAt (camPos + cam.transform.forward, prevUp); // keep straight
								} else {
									axis.z = 0;
									transform.Rotate (axis, -angleDiff, Space.World);
								}
//								dragDamping = 0;
								dragDampingStart = float.MaxValue;
							}
							break;
						}
					}
				}
				cam.transform.position = oldCamPos;
				transform.position = globePos;
			}

			if (dragDampingStart > 0) {
				float t = 1f - (Time.time - dragDampingStart) / dragDampingDuration;
//				if (++dragDamping < 20) {
				if (t >= 0 && t <= 1f) {
					if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
						transform.Rotate (cam.transform.up, dragDirection.x * t, Space.World);
						Vector3 axisY = Vector3.Cross (transform.position - cam.transform.position, cam.transform.up);
						transform.Rotate (axisY, dragDirection.y * t, Space.World);
					} else {
						if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
							cam.transform.RotateAround (transform.position, transform.up, -dragDirection.x * t);
						} else {
							RotateAround (cam.transform, transform.position, cam.transform.up, -dragDirection.x * t);
							RotateAround (cam.transform, transform.position, cam.transform.right, dragDirection.y * t);
						}
					}
				} else {
					dragDampingStart = 0;
				}
			}

			// Use mouse wheel to zoom in and out
			if (_allowUserZoom) {
				if (mouseIsOver || wheelAccel != 0) {
					float wheel = Input.GetAxis ("Mouse ScrollWheel");
					wheelAccel += wheel * (_invertZoomDirection ? -1 : 1);
				}

				// Support for pinch on mobile
				if (Input.touchSupported && Input.touchCount == 2) {
					// Store both touches.
					Touch touchZero = Input.GetTouch (0);
					Touch touchOne = Input.GetTouch (1);

					// Find the position in the previous frame of each touch.
					Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

					// Find the magnitude of the vector (the distance) between the touches in each frame.
					float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

					// Find the difference in the distances between each frame.
					float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
					deltaMagnitudeDiff = ApplyDragThreshold (deltaMagnitudeDiff, _mouseDragThreshold + 3f);
					if (!leftMouseButtonClick) {
						// Pass the delta to the wheel accel
						wheelAccel += deltaMagnitudeDiff;
					}
				}
			}

			if (wheelAccel != 0) {
				wheelAccel = Mathf.Clamp (wheelAccel, -0.1f, 0.1f);
				if (wheelAccel >= 0.01f || wheelAccel <= -0.01f) {
					float distFactor = Mathf.Min ((Vector3.Distance (cam.transform.position, transform.position) - radius) / radius, 1f);
					wheelAccel *= distFactor;

					if (_zoomMode == ZOOM_MODE.CAMERA_MOVES) {
						Vector3 camPos = cam.transform.position - (transform.position - cam.transform.position) * wheelAccel * _mouseWheelSensitivity;
						cam.transform.position = camPos;
						float radiusClip = _zoomMinDistance + radius + (cam.nearClipPlane + 0.01f);
						float radiusSqr = radiusClip * radiusClip;
						float camDistSqr = FastVector.SqrDistanceByValue (cam.transform.position, transform.position);
						if (camDistSqr < radiusSqr) {
							cam.transform.position = transform.position + (cam.transform.position - transform.position).normalized * radiusClip; // + 0.01f);
							wheelAccel = 0;
						} else {
							radiusSqr = _zoomMaxDistance + radius + cam.nearClipPlane;
							radiusSqr *= radiusSqr;
							if (camDistSqr > radiusSqr) {
								cam.transform.position = transform.position + (cam.transform.position - transform.position).normalized * Mathf.Sqrt (radiusSqr - 0.01f);
								wheelAccel = 0;
							}
						}
					} else {
						Vector3 earthPos = transform.position + (transform.position - cam.transform.position) * wheelAccel * _mouseWheelSensitivity;
						transform.position = earthPos;
						float radiusClip = _zoomMinDistance + radius + (cam.nearClipPlane + 0.01f);
						float radiusSqr = radiusClip * radiusClip;
						float camDistSqr = FastVector.SqrDistanceByValue (cam.transform.position, transform.position);
						if (camDistSqr < radiusSqr) {
							transform.position = cam.transform.position - (cam.transform.position - transform.position).normalized * radiusClip;
							wheelAccel = 0;
						} else {
							radiusSqr = _zoomMaxDistance + radius + cam.nearClipPlane;
							radiusSqr *= radiusSqr;
							if (camDistSqr > radiusSqr) {
								transform.position = cam.transform.position - (cam.transform.position - transform.position).normalized * Mathf.Sqrt (radiusSqr - 0.01f);
								wheelAccel = 0;
							}
						}

					}
					if (_dragConstantSpeed) {
						wheelAccel = 0;
					} else {
						wheelAccel *= _zoomDamping; // zoom deceleration
					}
				} else {
					wheelAccel = 0;
				}
			}

			if (_keepStraight && !flyToActive) {
				if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
					StraightenGlobe (SMOOTH_STRAIGHTEN_ON_POLES, true);
				} else {
					cam.transform.rotation = Quaternion.Lerp (cam.transform.rotation, GetCameraStraightLookRotation (), 0.3f);
				}
			}
		}

		void CheckUserInteractionInvertedMode () {
			Camera cam = mainCamera;

			// if mouse/finger is over map, implement drag and rotation of the world
			if (mouseIsOver) {
				// Use left mouse button and drag to rotate the world
				if (_allowUserRotation) {
					if (leftMouseButtonClick) {
						#if VR_GOOGLE
						mouseDragStart = GvrController.TouchPos;
						#else
						mouseDragStart = Input.mousePosition;
						if (Application.isMobilePlatform) {
							UpdateCursorLocation (); // _cursorLocation has not been set yet so we call CheckMousePos before any interaction
						}
						#endif
						mouseDragStartCursorLocation = _cursorLocation;
						mouseStartedDragging = true;
						hasDragged = false;
					} else if (mouseStartedDragging && leftMouseButtonPressed && Input.touchCount < 2) {
						if (_dragConstantSpeed) {
							if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY) {
								mouseDragStartCursorLocation.x = 0;
								_cursorLocation.x = 0;
							} else if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.X_AXIS_ONLY) {
								mouseDragStartCursorLocation.y = 0;
								_cursorLocation.y = 0;
							}
							if (CheckDragThreshold (mouseDragStart, Input.mousePosition, _mouseDragThreshold)) {
								float angle = Vector3.Angle (mouseDragStartCursorLocation, _cursorLocation);
								if (angle != 0) {
									hasDragged = true;
									if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
										Vector3 axis = Vector3.Cross (mouseDragStartCursorLocation, _cursorLocation);
										Vector3 angles = Quaternion.AngleAxis (-angle, axis).eulerAngles;
										angles.x *= -1;
										transform.Rotate (angles);
									} else {
										Vector3 axis = Vector3.Cross (transform.TransformPoint (mouseDragStartCursorLocation), transform.transform.TransformPoint (_cursorLocation));
										Vector3 angles = Quaternion.AngleAxis (-angle, axis).eulerAngles;
										cam.transform.Rotate (angles, Space.World);
									}
								}
							}
						} else {
							Vector3 referencePos = transform.position + cam.transform.forward * lastRestyleEarthNormalsScaleCheck.z * 0.5f;
							float distFactor = Vector3.Distance (cam.transform.position, referencePos);
							#if VR_GOOGLE
							dragDirection = (mouseDragStart - (Vector3)GvrController.TouchPos);
							dragDirection.y *= -1.0f;
																												dragDirection *= distFactor * _mouseDragSensitivity
							#else
							dragDirection = (Input.mousePosition - mouseDragStart);
							dragDirection.x = ApplyDragThreshold (dragDirection.x, _mouseDragThreshold);
							dragDirection.y = ApplyDragThreshold (dragDirection.y, _mouseDragThreshold);
							dragDirection *= 0.015f * distFactor * _mouseDragSensitivity;
							#endif
							if (_rotationAxisAllowed == WPM.ROTATION_AXIS_ALLOWED.X_AXIS_ONLY)
								dragDirection.y = 0;
							else if (_rotationAxisAllowed == ROTATION_AXIS_ALLOWED.Y_AXIS_ONLY)
								dragDirection.x = 0;


							if (dragDirection.x != 0 && dragDirection.y != 0) {
								hasDragged = true;
								if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
									transform.Rotate (Misc.Vector3up, dragDirection.x, Space.World);
									Vector3 axisY = Vector3.Cross (referencePos - cam.transform.position, Misc.Vector3up);
									transform.Rotate (axisY, dragDirection.y, Space.World);
								} else {
									dragDirection.x *= -1f;
									cam.transform.Rotate (dragDirection.y, dragDirection.x, 0, Space.Self);
								}
								dragDampingStart = Time.time;
							}
						}
						flyToActive = false;
					} else {
						if (mouseStartedDragging) {
							mouseStartedDragging = false;
							hasDragged = false;
						}
					}

					// Use right mouse button and drag to spin the world around z-axis
					if (rightMouseButtonPressed && Input.touchCount < 2 && !flyToActive) {
						if (_showProvinces && _provinceHighlightedIndex >= 0 && _centerOnRightClick && rightMouseButtonClick) {
							FlyToProvince (_provinceHighlightedIndex, 0.8f);
						} else if (_countryHighlightedIndex >= 0 && rightMouseButtonClick && _centerOnRightClick) {
							FlyToCountry (_countryHighlightedIndex, 0.8f);
						} else {
							Vector3 axis = (transform.position - cam.transform.position).normalized;
							transform.Rotate (axis, 2, Space.World);
						}
					}
				}
			}

			// Check special keys
			if (_allowUserKeys && _allowUserRotation) {
				bool pressed = false;
				dragDirection = Misc.Vector3zero;
				if (Input.GetKey (KeyCode.W)) {
					dragDirection += Misc.Vector3down;
					pressed = true;
				}
				if (Input.GetKey (KeyCode.S)) {
					dragDirection += Misc.Vector3up;
					pressed = true;
				}
				if (Input.GetKey (KeyCode.A)) {
					dragDirection += Misc.Vector3right;
					pressed = true;
				}
				if (Input.GetKey (KeyCode.D)) {
					dragDirection += Misc.Vector3left;
					pressed = true;
				}
				if (pressed) {
					Vector3 referencePos = transform.position + cam.transform.forward * lastRestyleEarthNormalsScaleCheck.z * 0.5f;
					dragDirection *= Vector3.Distance (cam.transform.position, referencePos) * _mouseDragSensitivity;
					transform.Rotate (Misc.Vector3up, dragDirection.x, Space.World);
					Vector3 axisY = Vector3.Cross (referencePos - cam.transform.position, Misc.Vector3up);
					transform.Rotate (axisY, dragDirection.y, Space.World);
					dragDampingStart = Time.time;
				}
			}

			if (dragDampingStart > 0) {
				float t = 1f - (Time.time - dragDampingStart) / dragDampingDuration;
				if (t >= 0 && t <= 1f) {
					if (_navigationMode == NAVIGATION_MODE.EARTH_ROTATES) {
						transform.Rotate (Misc.Vector3up, dragDirection.x * t, Space.World);
						Vector3 axisY = Vector3.Cross (transform.position - cam.transform.position, Misc.Vector3up);
						transform.Rotate (axisY, dragDirection.y * t, Space.World);
					} else {
						cam.transform.Rotate (dragDirection.y * t, dragDirection.x * t, 0, Space.Self);
					}
				} else {
					dragDampingStart = 0;
				}
			}

			// Check contraint
			if (constraintPositionEnabled) {
				// Check constraint around position
				Vector3 camPos = cam.transform.position;
				Ray ray = new Ray (cam.transform.position, cam.transform.forward);
				int hitCount = Physics.RaycastNonAlloc (ray.origin + ray.direction * transform.lossyScale.z * 1.5f, -ray.direction, hits, transform.lossyScale.z * 2f, layerMask);
				if (hitCount > 0) {
					Vector3 contraintWPos = transform.TransformPoint (constraintPosition) - camPos;
					for (int k = 0; k < hitCount; k++) {
						if (hits [k].collider.gameObject == gameObject) {
							Vector3 hitPoint = hits [k].point - camPos;
							Vector3 axis = Vector3.Cross (contraintWPos, hitPoint);
							float angleDiff = SignedAngleBetween (contraintWPos, hitPoint, axis);
							if (Mathf.Abs (angleDiff) > constraintAngle + 0.0001f) {
								if (angleDiff > 0) {
									angleDiff = constraintAngle - angleDiff;
								} else {
									angleDiff = angleDiff - constraintAngle;
								}
								if (_navigationMode == NAVIGATION_MODE.CAMERA_ROTATES) {
									Vector3 prevUp = cam.transform.up;
									cam.transform.Rotate (axis, angleDiff, Space.World);
									cam.transform.LookAt (camPos + cam.transform.forward, prevUp); // keep straight
								} else {
									axis.z = 0;
									transform.Rotate (axis, -angleDiff, Space.World);
								}
								dragDampingStart = 0;
							}
							break;
						}
					}
				}
			}

			// Use mouse wheel to zoom in and out
			if (_allowUserZoom) {
				if (mouseIsOver || wheelAccel != 0) {
					float wheel = Input.GetAxis ("Mouse ScrollWheel");
					wheelAccel += wheel * (_invertZoomDirection ? -1 : 1);
				}

				// Support for pinch on mobile
				if (Input.touchSupported && Input.touchCount == 2) {
					// Store both touches.
					Touch touchZero = Input.GetTouch (0);
					Touch touchOne = Input.GetTouch (1);

					// Find the position in the previous frame of each touch.
					Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

					// Find the magnitude of the vector (the distance) between the touches in each frame.
					float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

					// Find the difference in the distances between each frame.
					float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

					// Pass the delta to the wheel accel
					wheelAccel += deltaMagnitudeDiff;
				}
			}

			if (wheelAccel != 0) {
				wheelAccel = Mathf.Clamp (wheelAccel, -0.1f, 0.1f);
				if (wheelAccel >= 0.01f || wheelAccel <= -0.01f) {
					cam.fieldOfView = Mathf.Clamp (cam.fieldOfView + (90.0f * cam.fieldOfView / MAX_FIELD_OF_VIEW) * wheelAccel * _mouseWheelSensitivity, MIN_FIELD_OF_VIEW, MAX_FIELD_OF_VIEW);
					if (_dragConstantSpeed) {
						wheelAccel = 0;
					} else {
						wheelAccel *= _zoomDamping;
					}
				} else {
					wheelAccel = 0;
				}
			}

			if (_keepStraight && !flyToActive) {
				StraightenGlobe (SMOOTH_STRAIGHTEN_ON_POLES, true);
			}
		}

		void UpdateSurfaceCount () {
			if (_surfacesLayer != null)
				_surfacesCount = (_surfacesLayer.GetComponentsInChildren<Transform> ().Length - 1) / 2;
			else
				_surfacesCount = 0;
		}


		#endregion

		#region Highlighting

		public int layerMask { get { return 1 << mapUnityLayer; } }

		Ray GetRay () {
			Ray ray;
			Camera cam = mainCamera;

			if (_VREnabled) {
#if VR_GOOGLE
																if (GVR_Reticle != null && GVR_Reticle.gameObject.activeInHierarchy) {
																				Vector3 screenPoint = cam.WorldToScreenPoint (GVR_Reticle.position);
																				ray = cam.ScreenPointToRay (screenPoint);
																} else {
					RegisterVRPointers();
																				ray = new Ray (cam.transform.position, GvrController.Orientation * Vector3.forward);
																}
#elif VR_SAMSUNG_GEAR_CONTROLLER
				if (SVR_Laser != null && SVR_Laser.gameObject.activeInHierarchy) {
							ray = new Ray(SVR_Laser.transform.position, SVR_Laser.transform.forward);
				} else {
					RegisterVRPointers();
					ray = new Ray (cam.transform.position, cam.transform.rotation * Vector3.forward);
				}

#else
				ray = new Ray (cam.transform.position, cam.transform.rotation * Vector3.forward);
#endif
			} else {
				Vector3 mousePos = Input.mousePosition;
				ray = cam.ScreenPointToRay (mousePos);
			}
			return ray;
		}

		bool UpdateCursorLocation () {
			Ray ray = GetRay ();
			int hitCount;

			if (_earthInvertedMode) {
				hitCount = Physics.RaycastNonAlloc (ray.origin + ray.direction * transform.lossyScale.z * 1.5f, -ray.direction, hits, transform.lossyScale.z * 2f, layerMask);
			} else {
				hitCount = Physics.RaycastNonAlloc (ray.origin, ray.direction, hits, Mathf.Min (lastCameraDistanceSqr, 10000), layerMask);
			}
			if (hitCount > 0) {
				for (int k = 0; k < hitCount; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						_mouseIsOver = true;
						// Cursor follow
						if (_enableCountryHighlight || _dragConstantSpeed || (_cursorFollowMouse && _showCursor) || OnClick != null) { // need the cursor location for highlighting test and constant drag speed
							if (_cursorFollowMouse) {
								cursorLocation = transform.InverseTransformPoint (hits [k].point);
							} else {
								_cursorLocation = transform.InverseTransformPoint (hits [k].point);
							}
						}
						return true;
					}
				}
			}
			_mouseIsOver = false;
			return false;
		}


		void CheckMousePos () {

			if (UpdateCursorLocation ()) {
				// verify if hitPos is inside any country polygon
				int c, cr;
				if (GetCountryUnderMouse (cursorLocation, out c, out cr)) {
					bool ignoreCountryByEvent = false;
					if (c != _countryHighlightedIndex || (c == _countryHighlightedIndex && cr != _countryRegionHighlightedIndex)) {
						if (OnCountryBeforeEnter != null) {
							OnCountryBeforeEnter (c, cr, ref ignoreCountryByEvent);
						}
						if (!ignoreCountryByEvent) {
							HighlightCountryRegion (c, cr, false, _showOutline, _outlineColor);

							// Raise enter event
							if (OnCountryEnter != null)
								OnCountryEnter (c, cr);
						}
					}
					if (!ignoreCountryByEvent) {
						// if show provinces is enabled, then we draw provinces borders
						if (_showProvinces && _countries [c].allowShowProvinces) {
							mDrawProvinces (_countryHighlightedIndex, false, false); // draw provinces borders if not drawn
							int p, pr;
							// and now, we check if the mouse if inside a province, so highlight it
							if (GetProvinceUnderMouse (c, cursorLocation, out p, out pr)) {
								bool ignoreByEvent = false;
								if (OnProvinceBeforeEnter != null) {
									OnProvinceBeforeEnter (p, pr, ref ignoreByEvent);
								}
								if (p != _provinceHighlightedIndex || (p == _provinceHighlightedIndex && pr != _provinceRegionHighlightedIndex)) {
									HideProvinceRegionHighlight ();

									if (!ignoreByEvent) {
										// Raise enter event
										if (OnProvinceEnter != null)
											OnProvinceEnter (p, pr);
										HighlightProvinceRegion (p, pr, false);
									}
								}
							} else {
								HideProvinceRegionHighlight ();
							}
						}
						// if show cities is enabled, then check if mouse is over any city
						if (_showCities) {
							int ci;
							if (GetCityUnderMouse (c, cursorLocation, out ci)) {
								if (ci != _cityHighlightedIndex) {
									HideCityHighlight ();

									// Raise enter event
									if (OnCityEnter != null)
										OnCityEnter (ci);
								}
								HighlightCity (ci);
							} else if (_cityHighlightedIndex >= 0) {
								HideCityHighlight ();
							}
						}
						return;
					}
				}
			}
			HideCountryRegionHighlight ();
			if (!_drawAllProvinces)
				HideProvinces ();
		}

		#endregion


		#region Geometric functions

		float SignedAngleBetween (Vector3 a, Vector3 b, Vector3 n) {
			// angle in [0,180]
			float angle = Vector3.Angle (a, b);
			float sign = Mathf.Sign (Vector3.Dot (n, Vector3.Cross (a, b)));

			// angle in [-179,180]
			float signed_angle = angle * sign;

			return signed_angle;
		}

		Quaternion GetQuaternion (Vector3 point) {
			Camera cam = mainCamera;
			Quaternion oldRotation = transform.localRotation;
			Quaternion q;
			// center destination
			if (_earthInvertedMode) {
				cam.transform.LookAt (point);
				Vector3 angles = cam.transform.localRotation.eulerAngles;
				cam.transform.localRotation = Quaternion.Euler (new Vector3 (angles.x, -angles.y, angles.z));
				q = Quaternion.Inverse (cam.transform.localRotation);
				cam.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
			} else {
				Vector3 v1 = point;
				Vector3 v2 = cam.transform.position - transform.position;
				float angle = Vector3.Angle (v1, v2);
				Vector3 axis = Vector3.Cross (v1, v2);
				transform.localRotation = Quaternion.AngleAxis (angle, axis);
				// straighten view
				Vector3 v3 = Vector3.ProjectOnPlane (transform.up, v2);
				float angle2 = SignedAngleBetween (cam.transform.up, v3, v2);
				transform.Rotate (v2, -angle2, Space.World);
				q = transform.localRotation;
			}
			transform.localRotation = oldRotation;
			return q;
		}

		/// <summary>
		/// Better than Transform.RotateAround
		/// </summary>
		void RotateAround (Transform transform, Vector3 center, Vector3 axis, float angle) {
			Vector3 pos = transform.position;
			Quaternion rot = Quaternion.AngleAxis (angle, axis); // get the desired rotation
			Vector3 dir = pos - center;                         // find current direction relative to center
			dir = rot * dir;                                    // rotate the direction
			transform.position = center + dir;                  // define new position
			// rotate object to keep looking at the center:
			Quaternion myRot = transform.rotation;
			transform.rotation *= Quaternion.Inverse (myRot) * rot * myRot;
		}


		/// <summary>
		/// Internal usage. Checks quality of polygon points. Useful before using polygon clipping operations.
		/// Return true if there're changes.
		/// </summary>
		public bool RegionSanitize (Region region) {
			bool changes = false;
			if (tmpPoints == null) {
				tmpPoints = new List<Vector2> (region.latlon);
			} else {
				tmpPoints.Clear ();
			} 
			// removes points which are too near from others
			Vector2[] latlon = region.latlon;
			for (int k = 0; k < latlon.Length; k++) {
				bool validPoint = true;
				for (int j = k + 1; j < latlon.Length; j++) {
					float distance = (latlon [k].x - latlon [j].x) * (latlon [k].x - latlon [j].x) + (latlon [k].y - latlon [j].y) * (latlon [k].y - latlon [j].y);
					if (distance < 0.00000000001f) {
						validPoint = false;
						changes = true;
						break;
					}
				}
				if (validPoint) {
					tmpPoints.Add (latlon [k]);
				}
			}
			// remove crossing segments
			if (PolygonSanitizer.RemoveCrossingSegments (tmpPoints)) {
				changes = true;
			}
			if (changes) {
				region.latlon = tmpPoints.ToArray ();
			}
			region.sanitized = true;
			return changes;
		}

		/// <summary>
		/// Checks for the sanitized flag in regions list and invoke RegionSanitize on pending regions
		/// </summary>
		/// <param name="regions">Regions.</param>
		/// <param name="forceSanitize">If set to <c>true</c> it will perform the check regardless of the internal sanitized flag.</param>
		public void RegionSanitize (List<Region> regions, bool forceSanitize) {
			int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				Region region = regions [k];
				if (!region.sanitized || forceSanitize) {
					RegionSanitize (region);
				}
				if (region.latlon.Length < 3) { // remove invalid regions (<3 points)
					regions.RemoveAt (k);
					k--;
					regionCount--;
				}
			}
		}


		/// <summary>
		/// Makes a region collapse with the neigbhours frontiers - needed when merging two adjacent regions.
		/// neighbourRegion is not modified. Region points are the ones that can be modified to match neighbour border.
		/// </summary>
		/// <returns><c>true</c>, if region was changed. <c>false</c> otherwise.</returns>
		bool RegionMagnet (Region region, Region neighbourRegion) {

			const float tolerance = 1e-6f;
			int pointCount = region.latlon.Length;
			bool[] usedPoints = new bool[pointCount];
			int otherPointCount = neighbourRegion.latlon.Length;
			bool[] usedOtherPoints = new bool[otherPointCount];
			bool changes = false;

			for (int i = 0; i < pointCount; i++) { // maximum iterations = pointCount ; also avoid any potential (rare) infinite loop (good practice)
				float minDist = float.MaxValue;
				int selPoint = -1;
				int selOtherPoint = -1;
				// Search nearest pair of points
				for (int p = 0; p < pointCount; p++) {
					if (usedPoints [p])
						continue;
					Vector2 point0 = region.latlon [p];
					for (int o = 0; o < otherPointCount; o++) {
						if (usedOtherPoints [o])
							continue;
						Vector2 point1 = neighbourRegion.latlon [o];
						float dx = point0.x - point1.x;
						if (dx < 0)
							dx = -dx;
						if (dx < tolerance) {
							float dy = point0.y - point1.y;
							if (dy < 0)
								dy = -dy;
							if (dy < tolerance) {
								float dist = dx < dy ? dx : dy;
								if (dist <= 0) {
									// same point, ignore them now and in next iterations
									usedPoints [p] = true;
									usedOtherPoints [o] = true;
									selPoint = -1;
									break;
								} else if (dist < minDist) {
									minDist = dist;
									selPoint = p;
									selOtherPoint = o;
								}
							}
						}
					}
				}
				if (selPoint >= 0) {
					region.latlon [selPoint] = neighbourRegion.latlon [selOtherPoint];
					region.sanitized = false;
					usedPoints [selPoint] = true;
					usedOtherPoints [selOtherPoint] = true;
					changes = true;
				} else
					break; // exit loop, no more pairs
			}
			if (changes) {
				region.UpdateSpherePointsFromLatLon ();
			}
			return changes;
		}

		#endregion

		#region World Gizmos


		void CheckCursorVisibility () {
			if (cursorLayer != null && _showCursor) {
				if ((mouseIsOverUIElement || !mouseIsOver) && cursorLayer.activeSelf && !cursorAlwaysVisible) {   // not over globe?
					cursorLayer.SetActive (false);
				} else if (!mouseIsOverUIElement && mouseIsOver && !cursorLayer.activeSelf) {   // finally, should be visible?
					cursorLayer.SetActive (true);
				}
			}
		}

		void DrawCursor () {
			// Compute cursor dash lines
			float r = _earthInvertedMode ? 0.498f : 0.5f;
			Vector3 north = new Vector3 (0, r, 0);
			Vector3 south = new Vector3 (0, -r, 0);
			Vector3 west = new Vector3 (-r, 0, 0);
			Vector3 east = new Vector3 (r, 0, 0);
			Vector3 equatorFront = new Vector3 (0, 0, r);
			Vector3 equatorPost = new Vector3 (0, 0, -r);

			Vector3[] points = new Vector3[800];
			int[] indices0 = new int[400];
			int[] indices1 = new int[400];

			// Generate circumference V
			for (int k = 0; k < 400; k++) {
				indices0 [k] = k;
				indices1 [k] = k + 400;
			}
			for (int k = 0; k < 100; k++) {
				points [k] = Vector3.Lerp (north, equatorFront, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [100 + k] = Vector3.Lerp (equatorFront, south, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [200 + k] = Vector3.Lerp (south, equatorPost, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [300 + k] = Vector3.Lerp (equatorPost, north, k / 100.0f).normalized * r;
			}
			// Generate circumference H
			for (int k = 0; k < 100; k++) {
				points [400 + k] = Vector3.Lerp (equatorPost, west, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [500 + k] = Vector3.Lerp (west, equatorFront, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [600 + k] = Vector3.Lerp (equatorFront, east, k / 100.0f).normalized * r;
			}
			for (int k = 0; k < 100; k++) {
				points [700 + k] = Vector3.Lerp (east, equatorPost, k / 100.0f).normalized * r;
			}

			Transform t = transform.Find (CURSOR_LAYER);
			if (t != null)
				DestroyImmediate (t.gameObject);
			cursorLayer = new GameObject (CURSOR_LAYER);
			cursorLayer.transform.SetParent (transform, false);
			cursorLayer.layer = gameObject.layer;
			cursorLayer.transform.localPosition = Misc.Vector3zero;
			cursorLayer.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
			cursorLayer.SetActive (_showCursor);

			Mesh mesh = new Mesh ();
			mesh.vertices = points;
			mesh.subMeshCount = 2;
			mesh.SetIndices (indices0, MeshTopology.LineStrip, 0);
			mesh.SetIndices (indices1, MeshTopology.LineStrip, 1);
			mesh.RecalculateBounds ();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = cursorLayer.AddComponent<MeshFilter> ();
			mf.sharedMesh = mesh;

			MeshRenderer mr = cursorLayer.AddComponent<MeshRenderer> ();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.sharedMaterials = new Material[] { cursorMat, cursorMat };
		}

		void DrawGrid () {
			DrawLatitudeLines ();
			DrawLongitudeLines ();
		}

		void DrawLatitudeLines () {
			// Generate latitude lines
			List<Vector3> points = new List<Vector3> ();
			List<int> indices = new List<int> ();
			float r = _earthInvertedMode ? 0.498f : 0.501f;
			int idx = 0;
			float m = _frontiersDetail == FRONTIERS_DETAIL.High ? 4.0f : 5.0f;

			for (float a = 0; a < 90; a += _latitudeStepping) {
				for (int h = 1; h >= -1; h--) {
					if (h == 0)
						continue;

					float angle = a * Mathf.Deg2Rad;
					float y = h * Mathf.Sin (angle) * r;
					float r2 = Mathf.Cos (angle) * r;

					int step = Mathf.Min (1 + Mathf.FloorToInt (m * r / r2), 24);
					if ((100 / step) % 2 != 0)
						step++;

					for (int k = 0; k < 360 + step; k += step) {
						float ax = k * Mathf.Deg2Rad;
						float x = Mathf.Cos (ax) * r2;
						float z = Mathf.Sin (ax) * r2;
						points.Add (new Vector3 (x, y, z));
						if (k > 0) {
							indices.Add (idx);
							indices.Add (++idx);
						}
					}
					idx++;
					if (a == 0)
						break;
				}
			}

			Transform t = transform.Find (LATITUDE_LINES_LAYER);
			if (t != null)
				DestroyImmediate (t.gameObject);
			latitudeLayer = new GameObject (LATITUDE_LINES_LAYER);
			latitudeLayer.transform.SetParent (transform, false);
			latitudeLayer.layer = gameObject.layer;
			latitudeLayer.transform.localPosition = Misc.Vector3zero;
			latitudeLayer.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
			latitudeLayer.SetActive (_showLatitudeLines);

			Mesh mesh = new Mesh ();
			mesh.vertices = points.ToArray ();
			mesh.SetIndices (indices.ToArray (), MeshTopology.Lines, 0);
			mesh.RecalculateBounds ();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = latitudeLayer.AddComponent<MeshFilter> ();
			mf.sharedMesh = mesh;

			MeshRenderer mr = latitudeLayer.AddComponent<MeshRenderer> ();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.sharedMaterial = _gridMode == GRID_MODE.OVERLAY ? gridMatOverlay : gridMatMasked;

		}

		void DrawLongitudeLines () {
			// Generate longitude lines
			List<Vector3> points = new List<Vector3> ();
			List<int> indices = new List<int> ();
			float r = _earthInvertedMode ? 0.498f : 0.501f;
			int idx = 0;
			int step = _frontiersDetail == FRONTIERS_DETAIL.High ? 4 : 5;

			for (float a = 0; a < 180; a += 180 / _longitudeStepping) {
				float angle = a * Mathf.Deg2Rad;

				for (int k = 0; k < 360 + step; k += step) {
					float ax = k * Mathf.Deg2Rad;
					float x = Mathf.Cos (ax) * r * Mathf.Sin (angle); //Mathf.Cos (ax) * Mathf.Sin (angle) * r;
					float y = Mathf.Sin (ax) * r;
					float z = Mathf.Cos (ax) * r * Mathf.Cos (angle);
					points.Add (new Vector3 (x, y, z));
					if (k > 0) {
						indices.Add (idx);
						indices.Add (++idx);
					}
				}
				idx++;
			}

			Transform t = transform.Find (LONGITUDE_LINES_LAYER);
			if (t != null)
				DestroyImmediate (t.gameObject);
			longitudeLayer = new GameObject (LONGITUDE_LINES_LAYER);
			longitudeLayer.transform.SetParent (transform, false);
			longitudeLayer.layer = gameObject.layer;
			longitudeLayer.transform.localPosition = Misc.Vector3zero;
			longitudeLayer.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler(Misc.Vector3zero);
			longitudeLayer.SetActive (_showLongitudeLines);

			Mesh mesh = new Mesh ();
			mesh.vertices = points.ToArray ();
			mesh.SetIndices (indices.ToArray (), MeshTopology.Lines, 0);
			mesh.RecalculateBounds ();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = longitudeLayer.AddComponent<MeshFilter> ();
			mf.sharedMesh = mesh;

			MeshRenderer mr = longitudeLayer.AddComponent<MeshRenderer> ();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			//			mr.useLightProbes = false;
			mr.sharedMaterial = _gridMode == GRID_MODE.OVERLAY ? gridMatOverlay : gridMatMasked;

		}

		#endregion

		#region Overlay

		GameObject CreateOverlay () {

			if (!gameObject.activeInHierarchy)
				return null;

			// Prepare layer
			Transform t = transform.Find (WPM_OVERLAY_NAME);
			if (t == null) {
				overlayLayer = new GameObject (WPM_OVERLAY_NAME);
				overlayLayer.transform.SetParent (transform, false);
				overlayLayer.transform.position = new Vector3 (5000, 5000, 0);
			} else {
				overlayLayer = t.gameObject;
			}
			overlayLayer.transform.localScale = new Vector3 (1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f / transform.localScale.z);
			overlayLayer.layer = _overlayLayerIndex;

			// Sphere labels layer
			Material sphereOverlayMaterial = null;
			t = transform.Find (SPHERE_OVERLAY_LAYER_NAME);
			if (t != null) {
				Renderer r = t.gameObject.GetComponent<Renderer> ();
				if (r == null || r.sharedMaterial == null) {
					DestroyImmediate (t.gameObject);
					t = null;
				}
			}
			if (t == null) {
				sphereOverlayLayer = Instantiate (Resources.Load<GameObject> ("Prefabs/SphereOverlayLayer"));
				sphereOverlayLayer.hideFlags = HideFlags.DontSave;
				sphereOverlayLayer.name = SPHERE_OVERLAY_LAYER_NAME;
				sphereOverlayLayer.transform.SetParent (transform, false);
				sphereOverlayLayer.layer = gameObject.layer;
				sphereOverlayLayer.transform.localPosition = Misc.Vector3zero;
			} else {
				sphereOverlayLayer = t.gameObject;
				sphereOverlayLayer.SetActive (true);
			}

			// Material

			if (sphereOverlayMatDefault == null) {
				sphereOverlayMatDefault = Instantiate (Resources.Load<Material> ("Materials/SphereOverlay")) as Material;
				sphereOverlayMatDefault.hideFlags = HideFlags.DontSave;
			}
			sphereOverlayMaterial = sphereOverlayMatDefault;
			sphereOverlayLayer.GetComponent<Renderer> ().sharedMaterial = sphereOverlayMaterial;

			// Billboard
			GameObject billboard;
			t = overlayLayer.transform.Find ("Billboard");
			if (t == null) {
				billboard = Instantiate (Resources.Load<GameObject> ("Prefabs/Billboard"));
				billboard.name = "Billboard";
				billboard.transform.SetParent (overlayLayer.transform, false);
				billboard.transform.localPosition = Misc.Vector3zero;
				billboard.transform.localScale = new Vector3 (overlayWidth, overlayHeight, 1);
				billboard.layer = overlayLayer.layer;
			} else {
				billboard = t.gameObject;
			}

			// Render texture
			int imageWidth, imageHeight;
			switch (_labelsQuality) {
			case LABELS_QUALITY.Medium:
				imageWidth = 4096;
				imageHeight = 2048;
				break;
			case LABELS_QUALITY.High:
				imageWidth = 8192;
				imageHeight = 4096;
				break;
			default:
				imageWidth = 2048;
				imageHeight = 1024;
				break;
			}
			if (overlayRT != null && (overlayRT.width != imageWidth || overlayRT.height != imageHeight)) {
				overlayRT.Release ();
				DestroyImmediate (overlayRT);
				overlayRT = null;
			}

			Transform camTransform = overlayLayer.transform.Find (MAPPER_CAM);
			if (overlayRT == null) {
				overlayRT = new RenderTexture (imageWidth, imageHeight, 24);
				overlayRT.hideFlags = HideFlags.DontSave;
				overlayRT.filterMode = FilterMode.Trilinear;
				if (camTransform != null) {
					camTransform.GetComponent<Camera> ().targetTexture = overlayRT;
				}
			}
			//			lastOverlayHealthCheckTime = Time.time + 1f;			

			// Camera
			if (camTransform == null) {
				GameObject camObj = Instantiate (Resources.Load<GameObject> ("Prefabs/MapperCam"));
				camObj.name = MAPPER_CAM;
				camObj.transform.SetParent (overlayLayer.transform, false);
				camTransform = camObj.transform;
			}
			camTransform.gameObject.layer = _overlayLayerIndex;

			if (mapperCam == null) {
				mapperCam = camTransform.GetComponent<Camera> ();
				mapperCam.transform.localPosition = Vector3.back * 86.6f; // (10000.0f - 9999.13331f);
				mapperCam.aspect = 2;
				mapperCam.targetTexture = overlayRT;
				mapperCam.cullingMask = 1 << camTransform.gameObject.layer;
				mapperCam.fieldOfView = 60f;
				mapperCam.enabled = false;
				mapperCam.Render ();
			}

			// Assigns render texture to current material and recreates the camera
			sphereOverlayMaterial.mainTexture = overlayRT;

			// Reverse normals if inverted mode is enabled
			Drawing.ReverseSphereNormals (sphereOverlayLayer, _earthInvertedMode, _earthHighDensityMesh);
			AdjustSphereOverlayLayerScale ();
			return overlayLayer;
		}

		void AdjustSphereOverlayLayerScale () {
			if (_earthInvertedMode) {
				sphereOverlayLayer.transform.localScale = Misc.Vector3one * (0.998f - _labelsElevation * 0.5f);
			} else {
				sphereOverlayLayer.transform.localScale = Misc.Vector3one * (1.01f + _labelsElevation * 0.05f);
			}
		}

		void DestroyOverlay () {

			if (sphereOverlayLayer != null) {
				sphereOverlayLayer.SetActive (false);
			}

			if (overlayLayer != null) {
				DestroyImmediate (overlayLayer);
				overlayLayer = null;
			}

			GameObject oldWPMOverlay = GameObject.Find (WPM_OVERLAY_NAME);
			if (oldWPMOverlay != null) {
				DestroyImmediate (oldWPMOverlay);
			}
			if (overlayRT != null) {
				overlayRT.Release ();
				DestroyImmediate (overlayRT);
				overlayRT = null;
			}
		}

		#endregion

		#region Markers support

		void CheckMarkersLayer () {
			if (markersLayer == null) { // try to capture an existing marker layer
				Transform t = transform.Find ("Markers");
				if (t != null)
					markersLayer = t.gameObject;
			}
			if (markersLayer == null) { // create it otherwise
				markersLayer = new GameObject ("Markers");
				markersLayer.transform.SetParent (transform, false);
				markersLayer.transform.localPosition = Misc.Vector3zero;
			}
		}

		void CheckOverlayMarkersLayer () {
			GameObject overlayLayer = GetOverlayLayer (true);
			if (overlayMarkersLayer == null) { // try to capture an existing marker layer
				Transform t = overlayLayer.transform.Find (OVERLAY_MARKER_LAYER_NAME);
				if (t != null)
					overlayMarkersLayer = t.gameObject;
			}
			if (overlayMarkersLayer == null) { // create it otherwise
				overlayMarkersLayer = new GameObject (OVERLAY_MARKER_LAYER_NAME);
				overlayMarkersLayer.transform.SetParent (overlayLayer.transform, false);
				overlayMarkersLayer.transform.localPosition = Misc.Vector3zero;
				overlayMarkersLayer.layer = overlayLayer.layer;
			}
			requestMapperCamShot = true;
		}


		/// <summary>
		/// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
		/// </summary>
		/// <param name="marker">Your gameobject. Must be created by you.</param>
		/// <param name="sphereLocation">Location for the gameobject in sphere coordinates... [x,y,z] = (-0.5, 0.5).</param>
		/// <param name="markerScale">Scale to be applied to the gameobject. Your gameobject should have a scale of 1 and then pass to this function the desired scale.</param>
		/// <param name="isBillboard">If set to <c>true</c> the gameobject will be oriented to outside.</param>
		/// <param name="surfaceOffset">Surface offset. </param>
		/// </summary>
		void mAddMarker (GameObject marker, Vector3 sphereLocation, float markerScale, bool isBillboard, float surfaceOffset) {
			mAddMarker (marker, sphereLocation, markerScale, isBillboard, surfaceOffset, surfaceOffset == 0);
		}

		/// <summary>
		/// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
		/// </summary>
		/// <param name="marker">Your gameobject. Must be created by you.</param>
		/// <param name="sphereLocation">Location for the gameobject in sphere coordinates... [x,y,z] = (-0.5, 0.5).</param>
		/// <param name="markerScale">Scale to be applied to the gameobject. Your gameobject should have a scale of 1 and then pass to this function the desired scale.</param>
		/// <param name="isBillboard">If set to <c>true</c> the gameobject will be rotated 90º over local X-Axis.</param>
		/// <param name="surfaceOffset">Surface offset. </param>
		/// <param name="baselineAtBottom">If set to <c>true</c> the bottom of the gameobject boundary will sit over the surface or calculated height. If set to false, it's center will be used. Usually you pass true for buildings or stuff that sit on ground and false for anything that flies.</param>
		/// <param name="preserveOriginalRotation">If set to true, the object will finally rotated according to the original rotation</param>
		void mAddMarker (GameObject marker, Vector3 sphereLocation, float markerScale, bool isBillboard, float surfaceOffset, bool baselineAtBottom, bool preserveOriginalRotation = true) {
			// Try to get the height of the object

			float h = 0;
			float height = 0;
			if (baselineAtBottom) {
				if (marker.GetComponent<MeshFilter> () != null)
					height = marker.GetComponent<MeshFilter> ().sharedMesh.bounds.size.y;
				else if (marker.GetComponent<Collider> () != null)
					height = marker.GetComponent<Collider> ().bounds.size.y;
			}
			height += surfaceOffset;
			h = height * markerScale / sphereLocation.magnitude; // lift the marker so it appears on the surface of the globe

			CheckMarkersLayer ();

			// Assign marker parent, position, rotation
			if (marker.transform != markersLayer.transform) {
				marker.transform.SetParent (markersLayer.transform, false);
				// Does it have a collider? Then check it also has a rigidbody to enable interaction
				if (marker.GetComponent<Collider> () != null && marker.GetComponent<Rigidbody> () == null) {
					Rigidbody rb = marker.AddComponent<Rigidbody> ();
					rb.isKinematic = true;
				}
			}
			marker.transform.localPosition = _earthInvertedMode ? sphereLocation * (1.0f - h) : sphereLocation * (1.0f + h * 0.5f);

			// apply custom scale
			marker.transform.localScale = Misc.Vector3one * markerScale;

			Quaternion originalRotation = marker.transform.localRotation;

			if (_earthInvertedMode) {
				// flip localscale.x
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
				// once the marker is on the surface, rotate it so it looks to the surface
				marker.transform.LookAt (transform.position, transform.up);
				if (!isBillboard)
					marker.transform.Rotate (new Vector3 (90, 0, 0), Space.Self);
				// flip back localscale.x
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			} else {
				// once the marker is on the surface, rotate it so it looks to the surface
				marker.transform.LookAt (transform.position, transform.up);
				if (!isBillboard) {
					marker.transform.Rotate (new Vector3 (-90, 0, 0), Space.Self);
				}
			}

			if (preserveOriginalRotation) {
				marker.transform.Rotate (originalRotation.eulerAngles, Space.Self);
			}

		}

		/// <summary>
		/// Adds a polygon over the sphere.
		/// </summary>
		GameObject mAddMarkerCircle (MARKER_TYPE type, Vector3 sphereLocation, float kmRadius, float ringWidthStart, float ringWidthEnd, Color color) {
			GameObject marker = null;
			CheckOverlayMarkersLayer ();
			Vector2 position = Conversion.GetBillboardPosFromSpherePoint (sphereLocation);
			switch (type) {
			case MARKER_TYPE.CIRCLE:
				{
					float rw = 2.0f * Mathf.PI * EARTH_RADIUS_KM;
					float w = kmRadius / rw;
					w *= 2.0f * overlayWidth;
					float h = w;
					marker = Drawing.DrawCircle ("MarkerCircle", position, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 64, GetColoredMarkerOtherMaterial (color), false);
					if (marker != null) {
						marker.transform.SetParent (overlayMarkersLayer.transform, false);
						marker.transform.localPosition = new Vector3 (position.x, position.y, -0.01f);
						marker.layer = overlayMarkersLayer.layer;

						// Check seam
						Vector2 midPos = position;
						if (w + position.x > overlayWidth * 0.5f) {
							midPos.x -= overlayWidth;
						} else if (position.x - w < -overlayWidth * 0.5f) {
							midPos.x += overlayWidth;
						}
						if (midPos.x != position.x) {
							GameObject midCircle = Drawing.DrawCircle ("MarkerCircleMid", midPos, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 64, GetColoredMarkerOtherMaterial (color), false);
							midCircle.transform.SetParent (overlayMarkersLayer.transform, false);
							midCircle.transform.localPosition = new Vector3 (midPos.x, midPos.y, -0.01f);
							midCircle.transform.SetParent (marker.transform, true);
							midCircle.layer = overlayMarkersLayer.layer;
						}

					}
				}
				break;
			case MARKER_TYPE.CIRCLE_PROJECTED:
				{
					float rw = 2.0f * Mathf.PI * EARTH_RADIUS_KM;
					float w = kmRadius / rw;
					w *= 2.0f * overlayWidth;
					float h = w;
					marker = Drawing.DrawCircle ("MarkerCircle", position, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 128, GetColoredMarkerOtherMaterial (color), true);
					if (marker != null) {
						marker.transform.SetParent (overlayMarkersLayer.transform, false);
						marker.transform.localPosition = new Vector3 (position.x, position.y, -0.01f);
						marker.layer = overlayMarkersLayer.layer;

						// Check seam
						Vector2 midPos = position;
						if (position.x > 0) {
							midPos.x -= overlayWidth;
						} else {
							midPos.x += overlayWidth;
						}
						GameObject midCircle = Drawing.DrawCircle ("MarkerCircleMid", midPos, w, h, 0, Mathf.PI * 2.0f, ringWidthStart, ringWidthEnd, 128, GetColoredMarkerOtherMaterial (color), true);
						midCircle.transform.SetParent (overlayMarkersLayer.transform, false);
						midCircle.transform.localPosition = new Vector3 (midPos.x, midPos.y, -0.01f);
						midCircle.transform.SetParent (marker.transform, true);
						midCircle.layer = overlayMarkersLayer.layer;
					}
				}
				break;
			}
			return marker;
		}

		/// <summary>
		/// Adds a polygon over the sphere.
		/// </summary>
		GameObject mAddMarkerQuad (MARKER_TYPE type, Vector3 sphereLocationTopLeft, Vector3 sphereLocationBottomRight, Color fillColor, Color borderColor, float borderWidth) {
			GameObject marker = null;
			CheckOverlayMarkersLayer ();
			Vector2 position1 = Conversion.GetBillboardPosFromSpherePoint (sphereLocationTopLeft);
			Vector2 position2 = Conversion.GetBillboardPosFromSpherePoint (sphereLocationBottomRight);

			// clamp to edges of billboard
			if (Mathf.Abs (position2.x - position1.x) > overlayWidth * 0.5f) {
				if (position1.x > 0) {
					position2.x = overlayWidth * 0.5f;
				} else {
					position1.x = 0;
				}
			}

			switch (type) {
			case MARKER_TYPE.QUAD:
				marker = Drawing.DrawQuad ("MarkerQuad", position1, position2, GetColoredMarkerOtherMaterial (fillColor));
				marker.transform.SetParent (overlayMarkersLayer.transform, false);
				marker.transform.localPosition += Vector3.back * -0.01f;
				marker.layer = overlayMarkersLayer.layer;


				float dx = Mathf.Abs (position2.x - position1.x);
				float dy = Mathf.Abs (position2.y - position1.y);
				Vector3[] points = new Vector3[5];
				points [0] = new Vector3 (-dx * 0.5f, -dy * 0.5f);
				points [1] = points [0] + Misc.Vector3right * dx;
				points [2] = points [1] + Misc.Vector3up * dy;
				points [3] = points [2] - Misc.Vector3right * dx;
				points [4] = points [3] - Misc.Vector3up * dy;
				GameObject lines = Drawing.DrawLine (points, 5, borderWidth, GetColoredMarkerOtherMaterial (borderColor));
				lines.transform.SetParent (marker.transform, false);     // final parent
				lines.transform.localPosition += Vector3.back * -0.02f;
				lines.transform.localScale = new Vector3 (1f / (marker.transform.localScale.x + 0.001f), 1f / (marker.transform.localScale.y + 0.001f), 1f);
				break;
			}
			return marker;
		}


		#endregion

		#region GPS stuff

		IEnumerator CheckGPS () {
			if (!Application.isPlaying)
				yield break;
			while (_followDeviceGPS) {
				if (!flyToActive && Input.location.isEnabledByUser) {
					if (Input.location.status == LocationServiceStatus.Stopped) {
						Input.location.Start ();
					} else if (Input.location.status == LocationServiceStatus.Running) {
						float latitude = Input.location.lastData.latitude;
						float longitude = Input.location.lastData.longitude;
						FlyToLocation (latitude, longitude);
					}
				}
				yield return new WaitForSeconds (1f);
			}
		}

		void OnApplicationPause (bool pauseState) {

			if (!_followDeviceGPS || !Application.isPlaying)
				return;

			if (pauseState) {
				Input.location.Stop ();
			}

		}

		#endregion

	}

}