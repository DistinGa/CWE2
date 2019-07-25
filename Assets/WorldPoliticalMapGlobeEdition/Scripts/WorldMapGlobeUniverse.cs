// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPM {

	public enum SKYBOX_STYLE {
		UserDefined = 0,
		Basic = 1,
		MilkyWay = 2
	}

	/* Public WPM Class */
	public partial class WorldMapGlobe : MonoBehaviour {

		[SerializeField]
		Vector3 _earthScenicLightDirection = new Vector4 (-0.5f, 0.5f, -1f);

		public Vector3 earthScenicLightDirection {
			get { return _earthScenicLightDirection; }
			set {
				if (value != _earthScenicLightDirection) {
					_earthScenicLightDirection = value;
					isDirty = true;
					DrawAtmosphere ();
				}
			}
		}

		[SerializeField]
		Transform _sun;

		public Transform sun {
			get { return _sun; }
			set {
				if (value != _sun) {
					_sun = value;
					isDirty = true;
					RestyleEarth ();
				}
			}
		}


		[SerializeField]
		bool _showMoon = false;

		public bool showMoon {
			get { return _showMoon; }
			set {
				if (_showMoon != value) {
					_showMoon = value;
					isDirty = true;
					UpdateMoon ();
				}
			}
		}

		
		[SerializeField]
		bool _moonAutoScale = true;

		public bool moonAutoScale {
			get { return _moonAutoScale; }
			set {
				if (_moonAutoScale != value) {
					_moonAutoScale = value;
					isDirty = true;
					UpdateMoon ();
				}
			}
		}


		SKYBOX_STYLE _skyboxStyle = SKYBOX_STYLE.UserDefined;

		public SKYBOX_STYLE skyboxStyle {
			get { return _skyboxStyle; }
			set {
				if (_skyboxStyle != value) {
					_skyboxStyle = value;
					isDirty = true;
					UpdateSkybox ();
				}
			}
		}

		[SerializeField]
		bool _syncTimeOfDay = false;

		public bool syncTimeOfDay {
			get { return _syncTimeOfDay; }
			set {
				if (_syncTimeOfDay != value) {
					_syncTimeOfDay = value;
					if (_syncTimeOfDay) {
						if (!_earthStyle.isScatter () && !_earthStyle.isScenic ()) {
							earthStyle = EARTH_STYLE.NaturalHighResScenicScatterCityLights;
						}
					} else {
						TiltGlobe ();
					}
					isDirty = true;
				}
			}
		}


	}

}