//#define DEBUG_TILES
using UnityEngine;
using System;
using System.Linq;
using System.Text;

#if UNITY_WSA && !UNITY_EDITOR
using System.Threading.Tasks;
#else
using System.Threading;
#endif
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {

	public partial class WorldMapGlobe : MonoBehaviour {

		class ZoomLevelInfo {
			public int xMax, yMax;
			public GameObject tilesContainer;
			public int zoomLevelHash, yHash;
		}

		/// <summary>
		/// This is the minimum zoom level for tiles download. TILE_MIN_ZOOM_LEVEL must stay at 5. A lower value produces tiles that are too big to adapt to the sphere shape resulting in intersection artifacts with higher zoom levels.
		/// </summary>
		public const int TILE_MIN_ZOOM_LEVEL = 5;

		/// <summary>
		/// TILE_MAX_ZOOM_LEVEL can be increased if needed
		/// </summary>
		public const int TILE_MAX_ZOOM_LEVEL = 19;

		const int TILE_MIN_SIZE = 256;
		const string PREFIX_MIN_ZOOM_LEVEL = "z5_";
		const float TILE_MAX_QUEUE_TIME = 10;
		const string TILES_ROOT = "Tiles";
		int[] tileIndices = new int[6] { 2, 3, 0, 2, 0, 1 };
		Vector2[] tileUV = new Vector2[4] {
			Misc.Vector2up,
			Misc.Vector2one,
			Misc.Vector2right,
			Misc.Vector2zero
		};
		Vector4[] placeHolderUV = new Vector4[] {
			new Vector4 (0, 0.5f, 0.5f, 1f),
			new Vector4 (0.5f, 0.5f, 1f, 1f),
			new Vector4 (0, 0f, 0.5f, 0.5f),
			new Vector4 (0.5f, 0, 1f, 0.5f)
		};
		Color[][] meshColors = new Color[][] {
			new Color[] {
				new Color (1, 0, 0, 0),
				new Color (1, 0, 0, 0),
				new Color (1, 0, 0, 0),
				new Color (1, 0, 0, 0)
			},
			new Color[] {
				new Color (0, 1, 0, 0),
				new Color (0, 1, 0, 0),
				new Color (0, 1, 0, 0),
				new Color (0, 1, 0, 0)
			},
			new Color[] {
				new Color (0, 0, 1, 0),
				new Color (0, 0, 1, 0),
				new Color (0, 0, 1, 0),
				new Color (0, 0, 1, 0)
			},
			new Color[] {
				new Color (0, 0, 0, 1),
				new Color (0, 0, 0, 1),
				new Color (0, 0, 0, 1),
				new Color (0, 0, 0, 1)
			}
		};
		Vector2[] offsets = new Vector2[4] {
			new Vector2 (0, 0),
			new Vector2 (0.99f, 0),
			new Vector2 (0.99f, 0.99f),
			new Vector2 (0, 0.99f)
		};
		Color color1000 = new Color (1, 0, 0, 0);
		int _concurrentLoads;
		int _currentZoomLevel;
		int _webDownloads, _cacheLoads, _resourceLoads;
		long _webDownloadTotalSize, _cacheLoadTotalSize;
		int _tileSize = 0;
		string _tileServerCopyrightNotice;
		string _tileLastError;
		DateTime _tileLastErrorDate;
		Dictionary<int, TileInfo> cachedTiles;
		ZoomLevelInfo[] zoomLevelsInfo = new ZoomLevelInfo[20];
		List<TileInfo> loadQueue, newQueue;
		List<TileInfo> inactiveTiles;
		bool shouldCheckTiles, resortTiles;
		Material tileMatRef, tileMatTransRef;
		Transform tilesRoot;
		Renderer northPoleObj, southPoleObj;
		int subserverSeq;
		long _tileCurrentCacheUsage;
		FileInfo[] cachedFiles;
		int gcCount;
		float currentTileSize;
		Plane[] cameraPlanes;
		float lastDisposalTime;
		Texture2D currentEarthTexture;
		int spreadLoadAmongFrames;
		Camera currentCamera;
		Vector3 currentCameraPosition, currentCameraForward, globePos;
		string cachePath;
		float localScaleFactor;

		void InitTileSystem () {
			_tileServerCopyrightNotice = GetTileServerCopyrightNotice (_tileServer);

			cachePath = Application.persistentDataPath + "/TilesCache";
			if (!Directory.Exists (cachePath)) {
				Directory.CreateDirectory (cachePath);
			}

			if (!Application.isPlaying)
				return;

			if (_tileTransparentLayer) {
				tileMatRef = Resources.Load<Material> ("Materials/TileOverlayAlpha") as Material;
				tileMatTransRef = Resources.Load<Material> ("Materials/TileOverlayTransAlpha") as Material;
				Color alpha = new Color (1f, 1f, 1f, _tileMaxAlpha);
				tileMatRef.color = alpha;
				tileMatTransRef.color = alpha;
			} else {
				tileMatRef = Resources.Load<Material> ("Materials/TileOverlay") as Material;
				tileMatTransRef = Resources.Load<Material> ("Materials/TileOverlayTrans") as Material;
			}
			cameraPlanes = new Plane[6];

			if (_earthRenderer != null && _earthRenderer.sharedMaterial != null) {
				currentEarthTexture = (Texture2D)_earthRenderer.sharedMaterial.mainTexture;
			} else {
				currentEarthTexture = Texture2D.whiteTexture;
			}

			_tileSize = 0;

			InitZoomLevels ();
			if (loadQueue != null)
				loadQueue.Clear ();
			else
				loadQueue = new List<TileInfo> ();
			if (inactiveTiles != null)
				inactiveTiles.Clear ();
			else
				inactiveTiles = new List<TileInfo> ();
			if (cachedTiles != null)
				cachedTiles.Clear ();
			else
				cachedTiles = new Dictionary<int, TileInfo> ();

			if (Application.isPlaying)
				PurgeCacheOldFiles ();

			if (tilesRoot == null) {
				tilesRoot = transform.Find (TILES_ROOT);
			}
			if (tilesRoot != null)
				DestroyImmediate (tilesRoot.gameObject);
			if (tilesRoot == null) {
				GameObject tilesRootObj = new GameObject (TILES_ROOT);
				tilesRoot = tilesRootObj.transform;
				tilesRoot.SetParent (transform, false);
			}
			shouldCheckTiles = true;

		}

		void DestroyTiles () {
			if (tilesRoot != null)
				DestroyImmediate (tilesRoot.gameObject);
			if (northPoleObj != null) {
				DestroyImmediate (northPoleObj);
				northPoleObj = null;
			}
			if (southPoleObj != null) {
				DestroyImmediate (southPoleObj);
				southPoleObj = null;
			}
		}

		/// <summary>
		/// Reloads tiles
		/// </summary>
		public void ResetTiles () {
			DestroyTiles ();
			InitTileSystem ();
		}

		void PurgeCacheOldFiles () {
			PurgeCacheOldFiles (_tileMaxLocalCacheSize);
		}

		public void TileRecalculateCacheUsage () {
			_tileCurrentCacheUsage = 0;
			if (!Directory.Exists (cachePath))
				return;
			DirectoryInfo dir = new DirectoryInfo (cachePath);
			cachedFiles = dir.GetFiles ().OrderBy (p => p.LastAccessTime).ToArray ();
			for (int k = 0; k < cachedFiles.Length; k++) {
				_tileCurrentCacheUsage += cachedFiles [k].Length;
			}
		}

		/// <summary>
		/// Purges the cache old files.
		/// </summary>
		/// <param name="maxSize">Max size is in Mb.</param>
		void PurgeCacheOldFiles (long maxSize) {
			_tileCurrentCacheUsage = 0;
			if (!Directory.Exists (cachePath))
				return;
			DirectoryInfo dir = new DirectoryInfo (cachePath);
			// Delete old jpg files
			FileInfo[] jpgs = dir.GetFiles ("*.jpg.*");
			for (int k = 0; k < jpgs.Length; k++) {
				jpgs [k].Delete ();
			}

			cachedFiles = dir.GetFiles ().OrderBy (p => p.LastAccessTime).ToArray ();
			maxSize *= 1024 * 1024;
			for (int k = 0; k < cachedFiles.Length; k++) {
				_tileCurrentCacheUsage += cachedFiles [k].Length;
			}
			if (_tileCurrentCacheUsage <= maxSize)
				return;

			// Purge files until total size gets under max cache size
			for (int k = 0; k < cachedFiles.Length; k++) {
				if (_tilePreloadTiles && cachedFiles [k].Name.StartsWith (PREFIX_MIN_ZOOM_LEVEL)) {
					continue;
				}
				_tileCurrentCacheUsage -= cachedFiles [k].Length;
				cachedFiles [k].Delete ();
				if (_tileCurrentCacheUsage <= maxSize)
					return;
			}
		}

		void InitZoomLevels () {
			for (int k = 0; k < zoomLevelsInfo.Length; k++) {
				ZoomLevelInfo zi = new ZoomLevelInfo ();
				zi.xMax = (int)Mathf.Pow (2, k);
				zi.yMax = zi.xMax;
				zi.zoomLevelHash = (int)Mathf.Pow (4, k);
				zi.yHash = (int)Mathf.Pow (2, k);
				zoomLevelsInfo [k] = zi;
			}
		}

		void LateUpdateTiles () {
			if (!Application.isPlaying || cachedTiles == null)
				return;

			if (Time.time - lastDisposalTime > 3) {
				lastDisposalTime = Time.time;
				MonitorInactiveTiles ();
			}

			if (shouldCheckTiles || flyToActive) {
				shouldCheckTiles = false;
				currentCamera = mainCamera;	// for optimization purposes
				currentCameraPosition = currentCamera.transform.position;
				currentCameraForward = currentCamera.transform.forward;
				globePos = transform.position;

				_currentZoomLevel = GetTileZoomLevel ();
				int startingZoomLevel = TILE_MIN_ZOOM_LEVEL - 1;
				ZoomLevelInfo zi = zoomLevelsInfo [startingZoomLevel];
				int currentLoadQueueSize = loadQueue.Count;

				int qCount = loadQueue.Count;
				for (int k = 0; k < qCount; k++) {
					loadQueue [k].visible = false;
				}

				#if !UNITY_WEBGL && !UNITY_IOS && !UNITY_WSA
				GeometryUtilityNonAlloc.CalculateFrustumPlanes (cameraPlanes, currentCamera.projectionMatrix * currentCamera.worldToCameraMatrix);
				#else
				cameraPlanes = GeometryUtility.CalculateFrustumPlanes (currentCamera);
				#endif

				localScaleFactor = transform.localScale.x * 0.01f;
				for (int k = 0; k < zi.xMax; k++) {
					for (int j = 0; j < zi.yMax; j++) {
						CheckTiles (null, _currentZoomLevel, k, j, startingZoomLevel, 0);
					}
				}

				if (currentLoadQueueSize != loadQueue.Count)
					resortTiles = true;
				if (resortTiles) {
					resortTiles = false;
					loadQueue.Sort ((TileInfo x, TileInfo y) => {
						if (x.distToCamera < y.distToCamera)
							return -1;
						else if (x.distToCamera > y.distToCamera)
							return 1;
						else
							return 0;
					});
				}
				// Ensure local cache max size is not exceeded
				long maxLocalCacheSize = _tileMaxLocalCacheSize * 1024 * 1024;
				if (cachedFiles != null && _tileCurrentCacheUsage > maxLocalCacheSize) {
					for (int f = 0; f < cachedFiles.Length; f++) {
						if (cachedFiles [f] != null && cachedFiles [f].Exists) {
							if (_tilePreloadTiles && cachedFiles [f].Name.StartsWith (PREFIX_MIN_ZOOM_LEVEL)) {
								continue;
							}
							_tileCurrentCacheUsage -= cachedFiles [f].Length;
							cachedFiles [f].Delete ();
						}
						if (_tileCurrentCacheUsage <= maxLocalCacheSize)
							break;
					}
				}

			}

			CheckTilesContent (_currentZoomLevel);

			spreadLoadAmongFrames = _tileMaxTileLoadsPerFrame;
		}

		void MonitorInactiveTiles () {
			int inactiveCount = inactiveTiles.Count;
			bool changes = false;
			bool releasedMemory = false;
			for (int k = 0; k < inactiveCount; k++) {
				TileInfo ti = inactiveTiles [k];
				if (ti == null || ti.gameObject == null || ti.visible || ti.texture == currentEarthTexture || ti.loadStatus != TILE_LOAD_STATUS.Loaded) {
					inactiveTiles [k] = null;
					ti.isAddedToInactive = false;
					changes = true;
					continue;
				}
				if (Time.time - ti.inactiveTime > _tileKeepAlive) {
					inactiveTiles [k] = null;
					ti.isAddedToInactive = false;
					ti.loadStatus = TILE_LOAD_STATUS.Inactive;
					// tile is now invisible, setup material for when it appears again:
					ti.ClearPlaceholderImage ();
					if (ti.source != TILE_SOURCE.Resources) {
						Destroy (ti.texture);
					}
					ti.texture = currentEarthTexture;
					// Reset parentcoords on children
					if (ti.children != null) {
						int cCount = ti.children.Count;
						for (int c = 0; c < cCount; c++) {
							TileInfo tiChild = ti.children [c];
							if (!tiChild.animationFinished) {
								tiChild.ClearPlaceholderImage ();
							}
						}

					}
					changes = true;
					releasedMemory = true;
				}
			}
			if (changes) {
				List<TileInfo> newInactiveList = new List<TileInfo> ();
				for (int k = 0; k < inactiveCount; k++) {
					if (inactiveTiles [k] != null)
						newInactiveList.Add (inactiveTiles [k]);
				}
				inactiveTiles.Clear ();
				inactiveTiles = newInactiveList;
				if (releasedMemory) {
					Resources.UnloadUnusedAssets ();
					GC.Collect ();
				}
			}
		}

		void CheckTilesContent (int currentZoomLevel) {
			int qCount = loadQueue.Count;
			bool cleanQueue = false;
			for (int k = 0; k < qCount; k++) {
				TileInfo ti = loadQueue [k];
				if (ti == null) {
					cleanQueue = true;
					continue;
				}
				if (ti.loadStatus == TILE_LOAD_STATUS.InQueue) {
					if (ti.zoomLevel <= currentZoomLevel && ti.visible) {
						if (_tilePreloadTiles && ti.zoomLevel == TILE_MIN_ZOOM_LEVEL && ReloadTextureFromCacheOrMarkForDownload (ti)) {
							loadQueue [k] = null;
							cleanQueue = true;
							continue;
						}
						if (_concurrentLoads <= _tileMaxConcurrentDownloads) {
							ti.loadStatus = TILE_LOAD_STATUS.Loading;
							_concurrentLoads++;
							StartCoroutine (LoadTileContentBackground (ti));
						}
					} else if (Time.time - ti.queueTime > TILE_MAX_QUEUE_TIME) {
						ti.loadStatus = TILE_LOAD_STATUS.Inactive;
						loadQueue [k] = null;
						cleanQueue = true;
					}
				}
			}

			if (cleanQueue) {
				if (newQueue == null) {
					newQueue = new List<TileInfo> (qCount);
				} else {
					newQueue.Clear ();
				}
				for (int k = 0; k < qCount; k++) {
					TileInfo ti = loadQueue [k];
					if (ti != null)
						newQueue.Add (ti);
				}
				loadQueue.Clear ();
				loadQueue.AddRange (newQueue);
			}
		}

		#if DEBUG_TILES
		GameObject root;

		void AddMark (Vector3 worldPos)
		{
			GameObject mark = GameObject.CreatePrimitive (PrimitiveType.Cube);
			mark.transform.SetParent (root.transform);
			mark.transform.position = worldPos;
			mark.transform.localScale = Vector3.one * 100f;
			mark.GetComponent<Renderer> ().material.color = Color.yellow;
		}
#endif

		void CheckTiles (TileInfo parent, int currentZoomLevel, int xTile, int yTile, int zoomLevel, int subquadIndex) {
			// Is this tile visible?
			TileInfo ti;
			int tileCode = GetTileHashCode (xTile, yTile, zoomLevel);
			if (!cachedTiles.TryGetValue (tileCode, out ti)) {
				ti = new TileInfo (xTile, yTile, zoomLevel, subquadIndex, currentEarthTexture);
				ti.parent = parent;
				if (parent != null) {
					if (parent.children == null)
						parent.children = new List<TileInfo> ();
					parent.children.Add (ti);
				}
				for (int k = 0; k < 4; k++) {
					Vector2 latlon = Conversion.GetLatLonFromTile (xTile + offsets [k].x, yTile + offsets [k].y, zoomLevel);
					ti.latlons [k] = latlon;
					Vector3 spherePos = Conversion.GetSpherePointFromLatLon (latlon);
					ti.spherePos [k] = spherePos;
				}
				cachedTiles [tileCode] = ti;
			}
			// Check if any tile corner is visible
			// Phase I
#if DEBUG_TILES
			if (ti.gameObject != null && ti.gameObject.GetComponent<TileInfoEx> ().debug) {
				Debug.Log ("this");
			}
#endif

			bool cornersOccluded = true;
			Vector3 minWorldPos = Misc.Vector3Max;
			Vector3 maxWorldPos = Misc.Vector3Min;
			Vector3 tmp = Misc.Vector3zero;
			for (int c = 0; c < 4; c++) {
				Vector3 wpos = transform.TransformPoint (ti.spherePos [c]);
				ti.cornerWorldPos [c] = wpos;
				if (wpos.x < minWorldPos.x)
					minWorldPos.x = wpos.x;
				if (wpos.y < minWorldPos.y)
					minWorldPos.y = wpos.y;
				if (wpos.z < minWorldPos.z)
					minWorldPos.z = wpos.z;
				if (wpos.x > maxWorldPos.x)
					maxWorldPos.x = wpos.x;
				if (wpos.y > maxWorldPos.y)
					maxWorldPos.y = wpos.y;
				if (wpos.z > maxWorldPos.z)
					maxWorldPos.z = wpos.z;
				if (cornersOccluded) {
					float radiusSqr = (wpos.x - globePos.x) * (wpos.x - globePos.x) + (wpos.y - globePos.y) * (wpos.y - globePos.y) + (wpos.z - globePos.z) * (wpos.z - globePos.z); //  Vector3.SqrMagnitude (wpos - globePos);
//																				Vector3 camDir = (currentCameraPosition - wpos).normalized;
					FastVector.NormalizedDirection (ref wpos, ref currentCameraPosition, ref tmp);
//																				Vector3 st = wpos + ndir * (0.01f * transform.localScale.x);
					Vector3 st = wpos;
					FastVector.Add (ref st, ref tmp, localScaleFactor);
					float mag = (st.x - globePos.x) * (st.x - globePos.x) + (st.y - globePos.y) * (st.y - globePos.y) + (st.z - globePos.z) * (st.z - globePos.z);
					if (mag > radiusSqr) {
						cornersOccluded = false;
					}
				}
			}

//												Bounds bounds = new Bounds ((minWorldPos + maxWorldPos) * 0.5f, maxWorldPos - minWorldPos);
			FastVector.Average (ref minWorldPos, ref maxWorldPos, ref tmp);
			Bounds bounds = new Bounds (tmp, maxWorldPos - minWorldPos);
			Vector3 tileMidPoint = bounds.center;
			// Check center of quad
			if (cornersOccluded) {
				float radiusSqr = (tileMidPoint.x - globePos.x) * (tileMidPoint.x - globePos.x) + (tileMidPoint.y - globePos.y) * (tileMidPoint.y - globePos.y) + (tileMidPoint.z - globePos.z) * (tileMidPoint.z - globePos.z); // Vector3.SqrMagnitude (tileMidPoint - globePos);
//																Vector3 camDir = (currentCameraPosition - tileMidPoint).normalized;
				FastVector.NormalizedDirection (ref tileMidPoint, ref currentCameraPosition, ref tmp);
//																Vector3 st = tileMidPoint + tmp * (0.01f * transform.localScale.x);
				Vector3 st = tileMidPoint;
				FastVector.Add (ref st, ref tmp, localScaleFactor);
				float mag = (st.x - globePos.x) * (st.x - globePos.x) + (st.y - globePos.y) * (st.y - globePos.y) + (st.z - globePos.z) * (st.z - globePos.z);
				if (mag > radiusSqr) {
					cornersOccluded = false;
				}
			}

#if DEBUG_TILES
			if (root == null) {
				root = new GameObject ();
				root.transform.SetParent (transform);
				root.transform.localPosition = Vector3.zero;
				root.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler (0, 0, 0);
			}
#endif

			bool insideViewport = false;
			float minX = currentCamera.pixelWidth * 2f, minY = currentCamera.pixelHeight * 2f;
			float maxX = -minX, maxY = -minY;
			if (!cornersOccluded) {
				// Phase II
				for (int c = 0; c < 4; c++) {
					Vector3 scrPos = currentCamera.WorldToScreenPoint (ti.cornerWorldPos [c]);
					insideViewport = insideViewport || (scrPos.z > 0 && scrPos.x >= 0 && scrPos.x < currentCamera.pixelWidth && scrPos.y >= 0 && scrPos.y < currentCamera.pixelHeight);
					if (scrPos.x < minX)
						minX = scrPos.x;
					if (scrPos.x > maxX)
						maxX = scrPos.x;
					if (scrPos.y < minY)
						minY = scrPos.y;
					if (scrPos.y > maxY)
						maxY = scrPos.y;
				}
				if (!insideViewport) {
					insideViewport = GeometryUtility.TestPlanesAABB (cameraPlanes, bounds);
				}
			}

			ti.insideViewport = insideViewport;
			ti.visible = false;
			if (insideViewport) {
				if (!ti.created) {
					CreateTile (ti);
				}

				if (!ti.gameObject.activeSelf) {
					ti.gameObject.SetActive (true);
				}

				// Manage hierarchy of tiles
				bool tileIsBig = false;
				FastVector.NormalizedDirection (ref globePos, ref tileMidPoint, ref tmp);
//																float dd = Vector3.Dot (currentCameraForward, (tileMidPoint - globePos).normalized);
				float dd = Vector3.Dot (currentCameraForward, tmp);
				if (dd > -0.8f || currentZoomLevel > 9) { // prevents big seams on initial zooms
					float aparentSize = Mathf.Max (maxX - minX, maxY - minY);
					tileIsBig = aparentSize > currentTileSize;
				} else {
					tileIsBig = ti.zoomLevel < currentZoomLevel;
				}

				#if DEBUG_TILES
				if (ti.gameObject != null) {
					ti.gameObject.GetComponent<TileInfoEx> ().bigTile = tileIsBig;
					ti.gameObject.GetComponent<TileInfoEx> ().zoomLevel = ti.zoomLevel;
				}
				#endif

				if ((tileIsBig || zoomLevel < TILE_MIN_ZOOM_LEVEL) && zoomLevel < _tileMaxZoomLevel) {
					// Load nested tiles
					CheckTiles (ti, currentZoomLevel, xTile * 2, yTile * 2, zoomLevel + 1, 0);
					CheckTiles (ti, currentZoomLevel, xTile * 2 + 1, yTile * 2, zoomLevel + 1, 1);
					CheckTiles (ti, currentZoomLevel, xTile * 2, yTile * 2 + 1, zoomLevel + 1, 2);
					CheckTiles (ti, currentZoomLevel, xTile * 2 + 1, yTile * 2 + 1, zoomLevel + 1, 3);
					ti.renderer.enabled = false;
				} else {
					ti.visible = true;
					
					// Show tile renderer
					if (!ti.renderer.enabled) {
						ti.renderer.enabled = true;
					}

					// If parent tile is loaded then use that as placeholder texture
					if (ti.zoomLevel > TILE_MIN_ZOOM_LEVEL && ti.parent.loadStatus == TILE_LOAD_STATUS.Loaded && !ti.placeholderImageSet) {
						ti.placeholderImageSet = true;
						ti.parentTextureCoords = placeHolderUV [ti.subquadIndex];
						ti.SetPlaceholderImage (ti.parent.texture);
					}

					if (ti.loadStatus == TILE_LOAD_STATUS.Loaded) {
						if (!ti.hasAnimated) {
							ti.hasAnimated = true;
							ti.Animate (1f, AnimationEnded);
						}
					} else if (ti.loadStatus == TILE_LOAD_STATUS.Inactive) {
						ti.distToCamera = FastVector.SqrDistance (ref ti.cornerWorldPos [0], ref currentCameraPosition) * ti.zoomLevel;
						ti.loadStatus = TILE_LOAD_STATUS.InQueue;
						ti.queueTime = Time.time;
						loadQueue.Add (ti);
					}
					if (ti.children != null) {
						for (int k = 0; k < 4; k++) {
							TileInfo tiChild = ti.children [k];
							HideTile (tiChild);
						}
					}
				}
			} else {
				HideTile (ti);
			}
		}

		void HideTile (TileInfo ti) {
			if (ti.gameObject != null && ti.gameObject.activeSelf) {
				ti.gameObject.SetActive (false);
				ti.visible = false;
				if (ti.loadStatus == TILE_LOAD_STATUS.Loaded && ti.zoomLevel >= TILE_MIN_ZOOM_LEVEL) {
					if (_tilePreloadTiles && ti.zoomLevel == TILE_MIN_ZOOM_LEVEL)
						return;

					if (!ti.isAddedToInactive) {
						ti.isAddedToInactive = true;
						inactiveTiles.Add (ti);
					}
					ti.inactiveTime = Time.time;
				}
			}
		}

		void AnimationEnded (TileInfo ti) {
			shouldCheckTiles = true;
			// Switch tile material to solid
			ti.renderer.sharedMaterial = ti.parent.normalMat;
		}

		int GetTileHashCode (int x, int y, int zoomLevel) {
			ZoomLevelInfo zi = zoomLevelsInfo [zoomLevel];
			if (zi == null)
				return 0;
			int xMax = zi.xMax;
			x = (x + xMax) % xMax;
			int hashCode = zi.zoomLevelHash + zi.yHash * y + x;
			return hashCode;
		}

		TileInfo GetTileInfo (int x, int y, int zoomLevel) {
			int tileCode = GetTileHashCode (x, y, zoomLevel);
			TileInfo ti = null;
			cachedTiles.TryGetValue (tileCode, out ti);
			return ti;
		}

		int GetTileZoomLevel () {
			// Get screen dimensions of central tile
			int zoomLevel0 = 1;
			int zoomLevel1 = TILE_MAX_ZOOM_LEVEL;
			int zoomLevel = TILE_MIN_ZOOM_LEVEL;
			Vector2 latLon = Conversion.GetLatLonFromSpherePoint (GetCurrentMapLocation ());
			int xTile, yTile;
			currentTileSize = _tileSize > TILE_MIN_SIZE ? _tileSize : TILE_MIN_SIZE;
			currentTileSize *= (3.0f - _tileResolutionFactor);
			float dist = 0;
			for (int i = 0; i < 5; i++) {
				zoomLevel = (zoomLevel0 + zoomLevel1) / 2;
				Conversion.GetTileFromLatLon (zoomLevel, latLon.x, latLon.y, out xTile, out yTile);
				Vector2 latLonTL = Conversion.GetLatLonFromTile (xTile, yTile, zoomLevel);
				Vector2 latLonBR = Conversion.GetLatLonFromTile (xTile + 0.99f, yTile + 0.99f, zoomLevel);
				Vector3 spherePointTL = Conversion.GetSpherePointFromLatLon (latLonTL);
				Vector3 spherePointBR = Conversion.GetSpherePointFromLatLon (latLonBR);
				Vector3 wposTL = currentCamera.WorldToScreenPoint (transform.TransformPoint (spherePointTL));
				Vector3 wposBR = currentCamera.WorldToScreenPoint (transform.TransformPoint (spherePointBR));
				dist = Mathf.Max (Mathf.Abs (wposBR.x - wposTL.x), Mathf.Abs (wposTL.y - wposBR.y));
				if (dist > currentTileSize) {
					zoomLevel0 = zoomLevel;
				} else {
					zoomLevel1 = zoomLevel;
				}
			}
			if (dist > currentTileSize)
				zoomLevel++;

			zoomLevel = Mathf.Clamp (zoomLevel, TILE_MIN_ZOOM_LEVEL, TILE_MAX_ZOOM_LEVEL);
			return zoomLevel;
		}

		void CreateTile (TileInfo ti) {
			Vector2 latLonTL = ti.latlons [0];
			Vector2 latLonBR;
			ZoomLevelInfo zi = zoomLevelsInfo [ti.zoomLevel];
			int tileCode = GetTileHashCode (ti.x + 1, ti.y + 1, ti.zoomLevel);
			TileInfo cachedTile;
			if (cachedTiles.TryGetValue (tileCode, out cachedTile)) {
				latLonBR = cachedTile.latlons [0];
			} else {
				latLonBR = Conversion.GetLatLonFromTile (ti.x + 1, ti.y + 1, ti.zoomLevel);
			}
			// Avoid seams on very close distance to surface
			if (ti.zoomLevel >= 16) {
				float tao = 0.000002f * (ti.zoomLevel - 15);
				latLonTL.x += tao;
				latLonTL.y -= tao;
				latLonBR.x -= tao;
				latLonBR.y += tao;
			}

			// Create container
			GameObject parentObj;
			if (ti.parent == null) {
				parentObj = zi.tilesContainer;
				if (parentObj == null) {
					parentObj = new GameObject ("Tiles" + ti.zoomLevel);
					parentObj.transform.SetParent (tilesRoot, false);
					zi.tilesContainer = parentObj;
				}
			} else {
				parentObj = ti.parent.gameObject;
			}

			// Prepare mesh vertices
			Vector3[] tileCorners = new Vector3[4];
			tileCorners [0] = Conversion.GetSpherePointFromLatLon (latLonTL);
			tileCorners [1] = Conversion.GetSpherePointFromLatLon (new Vector2 (latLonTL.x, latLonBR.y));
			tileCorners [2] = Conversion.GetSpherePointFromLatLon (latLonBR);
			tileCorners [3] = Conversion.GetSpherePointFromLatLon (new Vector2 (latLonBR.x, latLonTL.y));

			// Setup tile materials
			TileInfo parent = ti.parent != null ? ti.parent : ti;
			if (parent.normalMat == null) {
				parent.normalMat = Instantiate (tileMatRef);
				parent.normalMat.hideFlags = HideFlags.DontSave;
			}
			if (parent.transMat == null) {
				parent.transMat = Instantiate (tileMatTransRef);
				parent.transMat.hideFlags = HideFlags.DontSave;
			}
			
			Material tileMat = ti.zoomLevel < TILE_MIN_ZOOM_LEVEL ? parent.normalMat : parent.transMat;

			// UVs wrt Earth texture
			Vector2 tl = new Vector2 ((latLonTL.y + 180) / 360f, (latLonTL.x + 90) / 180f);
			Vector2 br = new Vector2 ((latLonBR.y + 180) / 360f, (latLonBR.x + 90) / 180f);
			if (tl.x > 0.5f && br.x < 0.5f) {
				br.x = 1f;
			}
			ti.worldTextureCoords = new Vector4 (tl.x, br.y, br.x, tl.y);
			ti.ClearPlaceholderImage ();

			if (ti.zoomLevel < TILE_MIN_ZOOM_LEVEL) {
				ti.loadStatus = TILE_LOAD_STATUS.Loaded;
			}
			ti.texture = currentEarthTexture;

			ti.renderer = CreateObject (parentObj.transform, "Tile", tileCorners, tileIndices, tileUV, tileMat, ti.subquadIndex);
			ti.gameObject = ti.renderer.gameObject;
			ti.renderer.enabled = false;
			ti.created = true;

#if DEBUG_TILES
			ti.gameObject.AddComponent<TileInfoEx> ();
#endif

		}

		internal IEnumerator LoadTileContentBackground (TileInfo ti) {
			yield return new WaitForEndOfFrame ();

			string url = GetTileURL (_tileServer, ti);
			if (string.IsNullOrEmpty (url)) {
				Debug.LogError ("Tile server url not set. Aborting");
				yield break;
			}

			long downloadedBytes = 0;
			string error = null;
			string filePath = "";
			byte[] textureBytes = null;
			ti.source = TILE_SOURCE.Unknown;

			// Check if tile is given by external event
			if (OnTileRequest != null) {
				if (OnTileRequest (ti.zoomLevel, ti.x, ti.y, out ti.texture, out error) && ti.texture != null) {
					ti.source = TILE_SOURCE.Resources;
				}
			}

			// Check if tile is in Resources
			if (ti.source == TILE_SOURCE.Unknown && _tileEnableOfflineTiles) {
				string path = GetTileResourcePath (ti.x, ti.y, ti.zoomLevel, false);
				ResourceRequest request = Resources.LoadAsync<Texture2D> (path);
				yield return request;
				if (request.asset != null) {
					ti.texture = (Texture2D)request.asset;
					ti.source = TILE_SOURCE.Resources;
				} else if (tileOfflineTilesOnly) {
					ti.texture = tileResourceFallbackTexture;
					ti.source = TILE_SOURCE.Resources;
				}
			}

            CustomWWW www = null;
			if (ti.source == TILE_SOURCE.Unknown) {
				www = getCachedWWW (url, ti);
				yield return www;
				error = www.error;
			}

			for (int tries = 0; tries < 100; tries++) {
				if (spreadLoadAmongFrames > 0)
					break;
				yield return new WaitForEndOfFrame ();
			}
			spreadLoadAmongFrames--;
			_concurrentLoads--;
			
			if (!ti.visible) {	// non visible textures are ignored
				ti.loadStatus = TILE_LOAD_STATUS.InQueue;
				yield break;
			}

			if (!string.IsNullOrEmpty (error)) {
				_tileLastError = "Error getting tile: " + error + " url=" + url;
				_tileLastErrorDate = DateTime.Now;
				if (_tileDebugErrors) {
					Debug.Log (_tileLastErrorDate + " " + _tileLastError);
				}
				ti.loadStatus = TILE_LOAD_STATUS.InQueue;
				yield break;
			}

			// Load texture
			if (ti.source != TILE_SOURCE.Resources) {
				downloadedBytes = www.bytesDownloaded;
				textureBytes = www.bytes;
				ti.texture = www.textureNonReadable;
				www.Dispose ();
				www = null;

				// Check texture consistency
				if (ti.loadedFromCache || _tileEnableLocalCache) {
					filePath = GetLocalFilePathForURL (url, ti);
				}

				if (ti.loadedFromCache && ti.texture.width <= 16) { // Invalid texture in local cache, retry
					if (File.Exists (filePath)) {
						File.Delete (filePath);
					}
					ti.loadStatus = TILE_LOAD_STATUS.Inactive;
					ti.queueTime = Time.time;
					yield break;
				}
			}

			ti.texture.wrapMode = TextureWrapMode.Clamp;
			_tileSize = ti.texture.width;

			// Save texture
			if (_tileEnableLocalCache && ti.source != TILE_SOURCE.Resources && !File.Exists (filePath)) {
				_tileCurrentCacheUsage += textureBytes.Length;
				BackgroundSaver saver = new BackgroundSaver (textureBytes, filePath);
				saver.Start ();
			}

			// Update stats
			switch (ti.source) {
			case TILE_SOURCE.Cache:
				_cacheLoads++;
				_cacheLoadTotalSize += downloadedBytes;
				break;
			case TILE_SOURCE.Resources:
				_resourceLoads++;
				break;
			default:
				_webDownloads++;
				_webDownloadTotalSize += downloadedBytes;
				break;
			}

			if (loadQueue.Contains (ti)) {
				loadQueue.Remove (ti);
			}

			FinishLoadingTile (ti);
		}

		void CreatePole (TileInfo ti) {

			Vector3 polePos;
			Vector3 latLon0;
			string name;
			bool isNorth = (ti.y == 0);
			if (isNorth) {
				if (northPoleObj != null)
					return;
				polePos = Misc.Vector3up * 0.5f;
				latLon0 = ti.latlons [0];
				name = "North Pole";
			} else {
				if (southPoleObj != null)
					return;
				polePos = Misc.Vector3down * 0.5f;
				latLon0 = ti.latlons [2];
				name = "South Pole";
			}
			Vector3 latLon3 = latLon0;
			float lonDX = 360f / zoomLevelsInfo [ti.zoomLevel].xMax;
			latLon3.y += lonDX;
			int steps = (int)(360f / lonDX);
			int vertexCount = steps * 3;
			List<Vector3> vertices = new List<Vector3> (vertexCount);
			List<int> indices = new List<int> (vertexCount);
			List<Vector2> uv = new List<Vector2> (vertexCount);
			for (int k = 0; k < steps; k++) {
				Vector3 p0 = Conversion.GetSpherePointFromLatLon (latLon0);
				Vector3 p1 = Conversion.GetSpherePointFromLatLon (latLon3);
				latLon0 = latLon3;
				latLon3.y += lonDX;
				vertices.Add (p0);
				vertices.Add (p1);
				vertices.Add (polePos);
				indices.Add (k * 3);
				if (isNorth) {
					indices.Add (k * 3 + 2);
					indices.Add (k * 3 + 1);
				} else {
					indices.Add (k * 3 + 1);
					indices.Add (k * 3 + 2);
				}
				uv.Add (Misc.Vector2zero);
				uv.Add (Misc.Vector2up);
				uv.Add (Misc.Vector2right);
			}
			Renderer obj = CreateObject (tilesRoot.transform, name, vertices.ToArray (), indices.ToArray (), uv.ToArray (), ti.parent.normalMat, 0);
			if (isNorth) {
				northPoleObj = obj;
			} else {
				southPoleObj = obj;
			}
		}

		Renderer CreateObject (Transform parent, string name, Vector3[] vertices, int[] indices, Vector2[] uv, Material mat, int subquadIndex) {
			GameObject obj = new GameObject (name);
			obj.transform.SetParent (parent, false);
			obj.transform.localPosition = Misc.Vector3zero;
			obj.transform.localScale = Misc.Vector3one;
			obj.transform.localRotation = Misc.QuaternionZero; //Quaternion.Euler (0, 0, 0);
			Mesh mesh = new Mesh ();
			mesh.vertices = vertices;
			mesh.triangles = indices;
			mesh.uv = uv;
			Color[] meshColor;
			if (vertices.Length != 4) {
				meshColor = new Color[vertices.Length];
				for (int k = 0; k < vertices.Length; k++)
					meshColor [k] = color1000;
			} else {
				meshColor = meshColors [subquadIndex];
			}
			mesh.colors = meshColor;
			MeshFilter mf = obj.AddComponent<MeshFilter> ();
			mf.sharedMesh = mesh;
			MeshRenderer mr = obj.AddComponent<MeshRenderer> ();
			mr.sharedMaterial = mat;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = false;
			return mr;
		}

		class BackgroundSaver {
			byte[] tex;
			string filePath;

			public BackgroundSaver (byte[] tex, string filePath) {
				this.tex = tex;
				this.filePath = filePath;
			}

			public void Start () {
				#if UNITY_WSA && !UNITY_EDITOR
                Task.Run(() => SaveTextureToCache());
				#elif UNITY_WEBGL
				SaveTextureToCache();
				#else
				Thread thread = new Thread (SaveTextureToCache);
				thread.Start ();
				#endif
			}

			void SaveTextureToCache () {
				File.WriteAllBytes (filePath, tex);
			}

		}

		StringBuilder filePathStr = new StringBuilder (250);

		string GetLocalFilePathForURL (string url, TileInfo ti) {
			filePathStr.Length = 0;
			filePathStr.Append (cachePath);
			filePathStr.Append ("/z");
			filePathStr.Append (ti.zoomLevel);
			filePathStr.Append ("_x");
			filePathStr.Append (ti.x);
			filePathStr.Append ("_y");
			filePathStr.Append (ti.y);
			filePathStr.Append ("_");
			filePathStr.Append (url.GetHashCode ());
			filePathStr.Append (".png");
			return filePathStr.ToString ();
		}


		public string GetTileResourcePath (int x, int y, int zoomLevel, bool fullPath = true) {
			filePathStr.Length = 0;
			if (fullPath) {
				filePathStr.Append (_tileResourcePathBase);
				filePathStr.Append ("/");
			}
			filePathStr.Append ("Tiles");
			filePathStr.Append ("/");
			filePathStr.Append ((int)_tileServer);
			filePathStr.Append ("/z");
			filePathStr.Append (zoomLevel);
			filePathStr.Append ("_x");
			filePathStr.Append (x);
			filePathStr.Append ("_y");
			filePathStr.Append (y);
			if (fullPath) {
				filePathStr.Append (".png");
			}
			return filePathStr.ToString ();
		}


		CustomWWW getCachedWWW (string url, TileInfo ti) {
			string filePath = GetLocalFilePathForURL (url, ti);
            CustomWWW www;
			bool useCached = false;
			useCached = _tileEnableLocalCache && System.IO.File.Exists (filePath);
			if (useCached) {
				if (!_tilePreloadTiles || !filePath.Contains (PREFIX_MIN_ZOOM_LEVEL)) {
					//check how old
					System.DateTime written = File.GetLastWriteTimeUtc (filePath);
					System.DateTime now = System.DateTime.UtcNow;
					double totalHours = now.Subtract (written).TotalHours;
					if (totalHours > 300) {
						File.Delete (filePath);
						useCached = false;
					}
				}
			}
			ti.source = useCached ? TILE_SOURCE.Cache : TILE_SOURCE.Online;
			if (useCached) {
#if UNITY_STANDALONE_WIN || UNITY_WSA
				string pathforwww = "file:///" + filePath;
#else
				string pathforwww = "file://" + filePath;
#endif
				www = new CustomWWW (pathforwww);
			} else {
				www = new CustomWWW (url);
			}
			return www;
		}

		bool ReloadTextureFromCacheOrMarkForDownload (TileInfo ti) {
			if (!_tileEnableLocalCache)
				return false;

			string url = GetTileURL (_tileServer, ti);
			if (string.IsNullOrEmpty (url)) {
				return false;
			}

			string filePath = GetLocalFilePathForURL (url, ti);
			if (System.IO.File.Exists (filePath)) {
				//check how old
				System.DateTime written = File.GetLastWriteTimeUtc (filePath);
				System.DateTime now = System.DateTime.UtcNow;
				double totalHours = now.Subtract (written).TotalHours;
				if (totalHours > 300) {
					File.Delete (filePath);
					return false;
				}
			} else {
				return false;
			}
			byte[] bb = System.IO.File.ReadAllBytes (filePath);
			ti.texture = new Texture2D (0, 0);
			ti.texture.LoadImage (bb);
			if (ti.texture.width <= 16) { // Invalid texture in local cache, retry
				if (File.Exists (filePath)) {
					File.Delete (filePath);
				}
				return false;
			}
			ti.texture.wrapMode = TextureWrapMode.Clamp;

			_cacheLoads++;
			_cacheLoadTotalSize += bb.Length;

			FinishLoadingTile (ti);
			return true;
		}

		void FinishLoadingTile (TileInfo ti) {
			// Good to go, update tile info
			ti.SetTexture (ti.texture);
			
			ti.loadStatus = TILE_LOAD_STATUS.Loaded;
			if (ti.zoomLevel >= TILE_MIN_ZOOM_LEVEL) {
				if (ti.y == 0 || ti.y == zoomLevelsInfo [ti.zoomLevel].yMax - 1) {
					CreatePole (ti);
				}
			}
			shouldCheckTiles = true;
		}



	}

}