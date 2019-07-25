using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WPM
{
	static class MenuExtensions
	{

		[MenuItem ("GameObject/3D Object/World Political Map Globe Edition", false)]
		static void CreateGlobeMenuOption (MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/WorldMapGlobe")) as GameObject;
			go.name = "WorldMapGlobe";
			Undo.RegisterCreatedObjectUndo (go, "Create " + go.name);
			if (Selection.activeTransform != null) {
				go.transform.SetParent (Selection.activeTransform, false);
				go.transform.localPosition = Vector3.zero;
			}
			go.transform.localRotation = Quaternion.Euler (0, 0, 0);
			go.transform.localScale = new Vector3 (1f, 1f, 1f);
			Selection.activeObject = go;
		}
	}

}