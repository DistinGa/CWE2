using UnityEngine;
using System.Collections;

namespace WPM
{
	public partial class WorldMapGlobe : MonoBehaviour
	{
		const string SPHERE_FOW_NAME = "SphereFoWLayer";
		GameObject sphereFoWLayer;
		Material sphereFoWMaterial;
		RenderTexture rtFoW;
		Material fowDrawMat;

		#region Fog of War handling

		void CreateFogOfWarLayer ()
		{
			
			if (!gameObject.activeInHierarchy)
				return;
			
			// Sphere FoW layer
			Transform t = transform.Find (SPHERE_FOW_NAME);
			if (t != null) {
				Renderer r = t.gameObject.GetComponent<Renderer> ();
				if (r == null || r.sharedMaterial == null) {
					DestroyImmediate (t.gameObject);
					t = null;
				}
			}

			if (t == null) {
				sphereFoWLayer = Instantiate (Resources.Load<GameObject> ("Prefabs/SphereFoWLayer"));
				sphereFoWLayer.hideFlags = HideFlags.DontSave;
				sphereFoWLayer.name = SPHERE_FOW_NAME;
				sphereFoWLayer.transform.SetParent (transform, false);
				sphereFoWLayer.layer = gameObject.layer;
				sphereFoWLayer.transform.localPosition = Misc.Vector3zero;
			} else {
				sphereFoWLayer = t.gameObject;
				sphereFoWLayer.SetActive (true);
			}
			
			// Material
			if (sphereFoWMaterial == null) {
				sphereFoWMaterial = Instantiate (Resources.Load<Material> ("Materials/SphereFoW")) as Material;
				sphereFoWMaterial.hideFlags = HideFlags.DontSave;
			}
			sphereFoWLayer.GetComponent<Renderer> ().sharedMaterial = sphereFoWMaterial;

			CheckFoWTexture();
		}

		void CheckFoWTexture() {
			if (sphereFoWMaterial==null) return;
			int res = (int)Mathf.Pow (2, _fogOfWarResolution);
			if (rtFoW!=null && rtFoW.width != res) {
				rtFoW.Release();
				DestroyImmediate(rtFoW);
			}
			if (rtFoW==null) {
				RenderTextureFormat rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf) ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGB32;
				rtFoW = new RenderTexture(res, res/2, 0, rtFormat);
				rtFoW.hideFlags = HideFlags.DontSave;
				SetFowAlpha(Misc.Vector2zero, 1, 2, 1);
			}
		}


		void DestroyFogOfWarLayer ()
		{
			if (sphereFoWLayer != null) {
				DestroyImmediate (sphereFoWLayer);
			}
			if (rtFoW!=null) {
				RenderTexture.active = null;
				rtFoW.Release();
				DestroyImmediate(rtFoW);
				rtFoW = null;
			}
			if (fowDrawMat!=null) {
				DestroyImmediate(fowDrawMat);
				fowDrawMat = null;
			}
			if (sphereFoWMaterial!=null) {
				DestroyImmediate(sphereFoWMaterial);
				sphereFoWMaterial = null;
			}
		}


		void DrawFogOfWar ()
		{
			if (_showFogOfWar) {
				CreateFogOfWarLayer ();
				if (sphereFoWMaterial!=null) {
					sphereFoWMaterial.SetFloat("_Alpha", _fogOfWarAlpha);
					sphereFoWMaterial.SetColor("_Color", _fogOfWarColor1);
					sphereFoWMaterial.SetColor("_Color2", _fogOfWarColor2);
					sphereFoWMaterial.SetFloat("_Elevation", _fogOfWarElevation);
				}
			} else {
				DestroyFogOfWarLayer ();
			}
		}

		void SetFowAlpha(Vector2 uv, float alpha, float radius, float strength) {

			if (fowDrawMat == null) {
				fowDrawMat = new Material(Shader.Find("World Political Map/FogOfWarPainter"));
				fowDrawMat.hideFlags = HideFlags.DontSave;
			}
			fowDrawMat.SetVector("_PaintData", new Vector4(uv.x, uv.y, radius * radius, alpha));
			fowDrawMat.SetFloat("_PaintStrength", strength);
			RenderTexture rt = RenderTexture.GetTemporary(rtFoW.width, rtFoW.height, 0, rtFoW.format);
			Graphics.Blit(rtFoW, rt, fowDrawMat);
			rtFoW.DiscardContents();
			Graphics.Blit(rt, rtFoW);
			RenderTexture.ReleaseTemporary(rt);
			sphereFoWMaterial.SetTexture("_MaskTex", rtFoW);

		}

		
		#endregion





		

	}
}