// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPM.Poly2Tri;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace WPM {

	public partial class WorldMapGlobe : MonoBehaviour {
								
		/// <summary>
		/// Gets a list of regions that overlap with a given region
		/// </summary>
		public List<Region>GetRegionsOverlap (Region region, bool includeProvinces = false) {
			List<Region> rr = new List<Region> ();
			for (int k = 0; k < _countries.Length; k++) {
				Country country = _countries [k];
				if (country.regions == null)
					continue;
				int rCount = country.regions.Count;
				for (int r = 0; r < rCount; r++) {
					Region otherRegion = country.regions [r];
					if (region.Intersects (otherRegion)) {
						rr.Add (otherRegion);
					}
				}
			}

			if (includeProvinces) {
				int provinceCount = provinces.Length; // triggers lazy load
				for (int k = 0; k < provinceCount; k++) {
					Province province = _provinces [k];
					if (province.regions == null)
						continue;
					int rCount = province.regions.Count;
					for (int r = 0; r < rCount; r++) {
						Region otherRegion = province.regions [r];
						if (region.Intersects (otherRegion)) {
							rr.Add (otherRegion);
						}
					}
				}
			}
			return rr;
		}



		/// <summary>
		/// Adds extra points if distance between 2 consecutive points exceed some threshold 
		/// </summary>
		/// <returns><c>true</c>, if region was smoothed, <c>false</c> otherwise.</returns>
		/// <param name="region">Region.</param>
		public bool RegionSmooth (Region region, float smoothDistance) {
			int lastPoint = region.latlon.Length - 1;
			bool changes = false;
			List<Vector2> newPoints = new List<Vector2> (lastPoint + 1);
			for (int k = 0; k <= lastPoint; k++) {
				Vector2 p0 = region.latlon [k];
				Vector2 p1;
				if (k == lastPoint) {
					p1 = region.latlon [0];
				} else {
					p1 = region.latlon [k + 1];
				}
				newPoints.Add (p0);
				float dist = (p0 - p1).magnitude;
				if (dist > smoothDistance) {
					changes = true;
					int steps = Mathf.FloorToInt (dist / smoothDistance);
					float inc = dist / (steps + 1);
					float acum = inc;
					for (int j = 0; j < steps; j++) {
						newPoints.Add (Vector2.Lerp (p0, p1, acum / dist));
						acum += inc;
					}
				}
				newPoints.Add (p1);
			}
			if (changes)
				region.latlon = newPoints.ToArray ();
			return changes;
		}


		/// <summary>
		/// Modifies the borders of a region so it matches the hexagonal grid.
		/// </summary>
		/// <param name="region">Region.</param>
		public void RegionClampToCells (Region region, List<int>cellIndices) {

			// Get minimal distance between 2 cells
			float threshold = (cells [0].latlonCenter - cells [0].neighbours [0].latlonCenter).magnitude * 0.5f;
			float thresholdSqr = threshold * threshold;

			// Smooth borders
			RegionSmooth (region, threshold);

			// Clamp points to nearest cell vertex
			int cc = cellIndices.Count;
			Vector2[] regionLatlon = region.latlon;
			int pointCount = regionLatlon.Length;
			for (int k = 0; k < pointCount; k++) {
				float minDist = float.MaxValue;
				Vector2 nearest = Misc.Vector2zero;
				for (int c = 0; c < cc; c++) {
					Cell cell = cells [cellIndices [c]];
					Vector2[] cellLatlon = cell.latlon;
					for (int v = 0; v < cellLatlon.Length; v++) {
						float dist = (cellLatlon [v].x - regionLatlon [k].x) * (cellLatlon [v].x - regionLatlon [k].x) + (cellLatlon [v].y - regionLatlon [k].y) * (cellLatlon [v].y - regionLatlon [k].y);
						if (dist < minDist) {
							minDist = dist;
							nearest = cellLatlon [v];
							if (minDist < thresholdSqr) {
								c = cc;
								break;
							}
						}
					}
				}
				regionLatlon [k] = nearest;
			}											
			region.UpdateRect ();
			RegionSanitize (region); // remove duplicates
		}

	}

}