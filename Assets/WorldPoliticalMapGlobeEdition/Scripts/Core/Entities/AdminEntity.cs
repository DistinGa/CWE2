using UnityEngine;
using System.Collections.Generic;

namespace WPM {
	public abstract class AdminEntity: IAdminEntity {

		/// <summary>
		/// Entity name (country or province).
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Setting hidden to true will hide completely the entity (border, label) and it won't be highlighted
		/// </summary>
		public bool hidden;

		protected List<Region> _regions;

		/// <summary>
		/// List of all regions for the admin entity.
		/// </summary>
		public abstract List<Region> regions { get; set; }

		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		/// <summary>
		/// Returns the region object which is the main region of the country
		/// </summary>
		public abstract Region mainRegion { get; }

		public virtual float mainRegionArea { get { return mainRegion.rect2DArea; } }

		/// <summary>
		/// Computed Rect area that includes all regions. Used to fast hovering.
		/// </summary>
		public virtual Rect regionsRect2D { get; set; }

		public virtual float regionsRect2DArea { get { return this.regionsRect2D.width * this.regionsRect2D.height; } }

		protected Vector2 _latlonCenter;

		/// <summary>
		/// Center of the admin entity in the plane
		/// </summary>
		public virtual Vector2 latlonCenter {
			get { return _latlonCenter; } 
			set {
				_latlonCenter = value;
				_sphereCenter = Conversion.GetSpherePointFromLatLon (_latlonCenter);
			}
		}

		protected Vector3 _sphereCenter;

		public virtual Vector3 sphereCenter { get { return _sphereCenter; } }

		/// <summary>
		/// Returns true if any region of this entity contains the position
		/// </summary>
		/// <param name="mapPosition">Map position.</param>
		public bool Contains (Vector2 mapPosition) {
			if (regions == null)
				return false;
			int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				if (regions [k].Contains (mapPosition)) {
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Returns true if any region of this entity overlaps a given region
		/// </summary>
		/// <param name="otherRegion">Other region.</param>
		public bool Overlaps (Region otherRegion) {
			if (regions == null)
				return false;
			int regionCount = regions.Count;
			for (int k = 0; k < regionCount; k++) {
				if (regions [k].Intersects (otherRegion)) {
					return true;
				}
			}
			return false;
		}

	}
}