using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
				/// <summary>
				/// Mount Point record. Mount points are stored in the mountPoints file, in packed string editable format inside Resources/Geodata folder.
				/// </summary>
				public partial class MountPoint {
		
								/// <summary>
								/// Name of this mount point.
								/// </summary>
								public string name;
		
								/// <summary>
								/// Type of mount point. This is an optional, user-defined integer value.
								/// </summary>
								public int type;
		
								/// <summary>
								/// The index of the country.
								/// </summary>
								public int countryIndex;
		
								/// <summary>
								/// The index of the province or -1 if the mount point is not linked to any province.
								/// </summary>
								public int provinceIndex;
		
								/// <summary>
								/// The location of the mount point on the sphere.
								/// </summary>
								public Vector3 unitySphereLocation;
		
								/// <summary>
								/// The custom tags stored in a dictionary object (both keys and values are strings). These are optionally, user-defined values.
								/// </summary>
								public Dictionary<string,string> customTags;


								public float latitude {
												get {
																return latlon.x;
												}
								}

								public float longitude {
												get {
																return latlon.y;
												}
								}

								Vector2 _latlon;

								public Vector2 latlon {
												get {
																if (latlonPending) {
																				UpdateLatitudeLongitude ();
																}
																return _latlon;
												}

								}


								float _latitude, _longitude;
								bool latlonPending;

								public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location, int type, Dictionary<string, string>tags) {
												this.name = name;
												this.countryIndex = countryIndex;
												this.provinceIndex = provinceIndex;
												this.unitySphereLocation = location;
												this.type = type;
												this.customTags = tags;
												this.latlonPending = true;
								}

								public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location, int type) : this (name, countryIndex, provinceIndex, location, type, new Dictionary<string,string> ()) {
								}

								public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location) : this (name, countryIndex, provinceIndex, location, 0, new Dictionary<string,string> ()) {
								}

								public MountPoint Clone () {
												// Clone dictionary
												Dictionary<string, string> tags = new Dictionary<string, string> (customTags.Count, customTags.Comparer);
												foreach (KeyValuePair<string, string> entry in customTags) {
																tags.Add (entry.Key, entry.Value);
												}
												MountPoint c = new MountPoint (name, countryIndex, provinceIndex, unitySphereLocation, type, tags);
												return c;
								}

								void UpdateLatitudeLongitude () {
												WorldMapGlobe map = WorldMapGlobe.instance;
												if (map == null)
																return;
												latlonPending = false;
												_latlon = Conversion.GetLatLonFromSpherePoint (unitySphereLocation);
								}
				}
}