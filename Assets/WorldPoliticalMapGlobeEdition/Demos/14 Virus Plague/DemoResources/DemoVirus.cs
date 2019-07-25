using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
	public class DemoVirus : MonoBehaviour {

		const int MAX_BOUNCES = 8;

		public Material circleMat, combineMat;
		public Texture2D earthMask;

		WorldMapGlobe map;
		GUIStyle buttonStyle;
		RenderTexture rtEarth, rtVirusMap, rtCombined;
		Material earthMat;
		int bounces;

		void Start () {
			buttonStyle = new GUIStyle ();
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 

			// Get map instance to Globe API methods
			map = WorldMapGlobe.instance;
			earthMat = map.earthMaterial;

			// Gets a copy of current Earth texture and make it available as render texture for faster blending
			rtEarth = new RenderTexture (2048, 1024, 0);
			Graphics.Blit (map.earthTexture, rtEarth);

			rtVirusMap = new RenderTexture (2048, 1024, 0);
			rtCombined = new RenderTexture (2048, 1024, 0);

			circleMat.SetTexture ("_MaskTex", earthMask); // to avoid painting over Sea

			StartPlague ();
		}

		void OnGUI () {
			GUIResizer.AutoResize ();
			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.5f);
			if (GUI.Button (new Rect (10, 10, 160, 30), "  Start Plague Again", buttonStyle)) {
				StartPlague ();
			}
		}

		void StartPlague () {
			rtVirusMap.Clear (false, true, Misc.ColorTransparent);

			// Get a random city
			int cityRandom = Random.Range (0, map.cities.Count);
			cityRandom = map.GetCityIndex ("Spain", "Madrid", "Madrid");
			Vector2 pointZero = map.cities [cityRandom].latlon;

			StartCoroutine (Spread (cityRandom));
		}



		IEnumerator Spread (int cityIndex) {
			City city = map.cities [cityIndex];
			Vector2 latlon = city.latlon;
			float radius = 0.002f;
			switch (city.cityClass) {
			case CITY_CLASS.REGION_CAPITAL:
				radius += 0.004f;
				break;
			case CITY_CLASS.COUNTRY_CAPITAL:
				radius += 0.007f;
				break;
			}
			radius += Random.value * 0.005f;
			
			WaitForEndOfFrame w = new WaitForEndOfFrame ();
			Vector2 uv = Conversion.GetUVFromLatLon (latlon.x, latlon.y);
			float startTime = Time.time;
			float t = 0;
			const float duration = 4f;
			do {
				t = (Time.time - startTime) / duration;
				if (t > 1f)
					t = 1f;
				rtVirusMap.Circle (uv, t * radius, circleMat);
				yield return w;


				if (Random.value > 0.97f && bounces < MAX_BOUNCES) {
					Bounce (latlon);
				}
			} while(t < 1f);
			Bounce (latlon);
			bounces--;
		}

		void Bounce (Vector2 latlonStart) {
			// Spread to another near city
			int anotherCity = 0;
			float minDist = float.MaxValue;
			for (int k=0;k<25;k++) {
				int c = Random.Range (0, map.cities.Count);
				float dist = map.calc.Distance (latlonStart, map.cities [c].latlon);
				if (dist < minDist) {
					anotherCity = c;
					minDist = dist;
				}
			}
			Vector2 dest = map.cities [anotherCity].latlon;
			LineMarkerAnimator line = map.AddLine (latlonStart, dest, Color.yellow, 0.1f, 2f, 0.05f, 0.1f);
			line.OnLineDrawingEnd += (LineMarkerAnimator lma) => {
				StartCoroutine (Spread (anotherCity));
			};
			bounces++;

		}


		void Update () {
			// Combine virus map with Earth texture
			combineMat.SetTexture ("_SecondTex", rtVirusMap);
			Graphics.Blit (rtEarth, rtCombined, combineMat);
			earthMat.mainTexture = rtCombined;
		}
	}


	public static class RenderTextureExtensions {

		public static void Clear (this RenderTexture rt, bool clearDepth, bool clearColor, Color backgroundColor) {
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			GL.Clear (clearDepth, clearColor, backgroundColor);
			RenderTexture.active = old;
		}

		public static void Circle (this RenderTexture rt, Vector2 uv, float radius, Material mat) {
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			mat.SetVector ("_CenterAndRadius", new Vector3 (uv.x, uv.y, radius * radius));
			Graphics.Blit (null, rt, mat);
			RenderTexture.active = old;
		}
	}
}
