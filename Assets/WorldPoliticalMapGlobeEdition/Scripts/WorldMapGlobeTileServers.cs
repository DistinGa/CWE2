using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace WPM {
	
	public enum TILE_SERVER {
		OpenStreeMap = 10,
		OpenStreeMapDE = 11,
		StamenToner = 20,
		StamenWaterColor = 21,
		StamenTerrain = 22,
		CartoLightAll = 40,
		CartoDarkAll = 41,
		CartoNoLabels = 42,
		CartoOnlyLabels = 43,
		CartoDarkNoLabels = 44,
		CartoDarkOnlyLabels = 45,
		WikiMediaAtlas = 50,
		ThunderForestLandscape = 60,
		OpenTopoMap = 70,
		MapBoxSatellite = 80,
		Sputnik = 100,
		AerisWeather = 110,
		Custom = 999
	}

	public partial class WorldMapGlobe : MonoBehaviour {

		public static string[] tileServerNames = new string[] {
			"Open Street Map",
			"Open Street Map (DE)",
			"Stamen Terrain",
			"Stamen Toner",
			"Stamen WaterColor",
			"Carto LightAll",
			"Carto DarkAll",
			"Carto No Labels",
			"Carto Only Labels",
			"Carto Dark No Labels",
			"Carto Dark Only Labels",
			"WikiMedia Atlas",
			"ThunderForest Landscape",
			"OpenTopoMap",
			"MapBox Satellite",
			"Sputnik",
			"AerisWeather",
			"Custom"
		};

		public static int[] tileServerValues = new int[] {
			(int)TILE_SERVER.OpenStreeMap,
			(int)TILE_SERVER.OpenStreeMapDE,
			(int)TILE_SERVER.StamenTerrain,
			(int)TILE_SERVER.StamenToner,
			(int)TILE_SERVER.StamenWaterColor,
			(int)TILE_SERVER.CartoLightAll,
			(int)TILE_SERVER.CartoDarkAll,
			(int)TILE_SERVER.CartoNoLabels,
			(int)TILE_SERVER.CartoOnlyLabels,
			(int)TILE_SERVER.CartoDarkNoLabels,
			(int)TILE_SERVER.CartoDarkOnlyLabels,
			(int)TILE_SERVER.WikiMediaAtlas,
			(int)TILE_SERVER.ThunderForestLandscape,
			(int)TILE_SERVER.OpenTopoMap,
			(int)TILE_SERVER.MapBoxSatellite,
			(int)TILE_SERVER.Sputnik,
			(int)TILE_SERVER.AerisWeather,
			(int)TILE_SERVER.Custom
		};


		public string GetTileServerCopyrightNotice (TILE_SERVER server) {

			string copyright;

			switch (_tileServer) {
			case TILE_SERVER.OpenStreeMap: 
				copyright = "Map tiles © OpenStreetMap www.osm.org/copyright";
				break;
			case TILE_SERVER.OpenStreeMapDE: 
				copyright = "Map tiles © OpenStreetMap www.osm.org/copyright";
				break;
			case TILE_SERVER.StamenToner: 
				copyright = "Map tiles by Stamen Design, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.StamenTerrain:
				copyright = "Map tiles by Stamen Design, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.StamenWaterColor:
				copyright = "Map tiles by Stamen Design, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoLightAll:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoDarkAll:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoNoLabels:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoOnlyLabels:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoDarkNoLabels:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.CartoDarkOnlyLabels:
				copyright = "Map tiles by Carto, under CC BY 3.0. Data by OpenStreetMap, under ODbL.";
				break;
			case TILE_SERVER.WikiMediaAtlas:
				copyright = "Map tiles © WikiMedia/Mapnik, Data © www.osm.org/copyright";
				break;
			case TILE_SERVER.ThunderForestLandscape:
				copyright = "Map tiles © www.thunderforest.com, Data © www.osm.org/copyright";
				break;
			case TILE_SERVER.OpenTopoMap:
				copyright = "Map tiles © OpenTopoMap, Data © www.osm.org/copyright";
				break;
			case TILE_SERVER.MapBoxSatellite:
				copyright = "Map tiles © MapBox";
				break;
			case TILE_SERVER.Sputnik:
				copyright = "Map tiles © Sputnik, Data © www.osm.org/copyright";
				break;
			case TILE_SERVER.AerisWeather:
				copyright = "Map tiles © Aeris Weather, www.aerisweather.com"; 
				break;
			case TILE_SERVER.Custom:
				copyright = "";
				break;
			default:
				Debug.LogError ("Tile server not defined: " + tileServer.ToString ());
				copyright = "";
				break;
			}

			return copyright;
		}

		public string GetTileURL (TILE_SERVER server, TileInfo ti) {

			string url;
			string[] subservers = { "a", "b", "c" };
			subserverSeq++;
			if (subserverSeq > 100000)
				subserverSeq = 0;

			switch (_tileServer) {
			case TILE_SERVER.OpenStreeMap: 
				url = "http://" + subservers [subserverSeq % 3] + ".tile.openstreetmap.org/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.OpenStreeMapDE: 
				url = "http://" + subservers [subserverSeq % 3] + ".tile.openstreetmap.de/tiles/osmde/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.StamenToner: 
				url = "http://tile.stamen.com/toner/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.StamenTerrain:
				url = "http://tile.stamen.com/terrain/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.StamenWaterColor:
				url = "http://tile.stamen.com/watercolor/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoLightAll:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/light_all/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoDarkAll:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/dark_all/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoNoLabels:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/light_nolabels/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoOnlyLabels:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/light_only_labels/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoDarkNoLabels:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/dark_nolabels/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.CartoDarkOnlyLabels:
				url = "http://" + subservers [subserverSeq % 3] + ".basemaps.cartocdn.com/dark_only_labels/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.WikiMediaAtlas:
				url = "http://" + subservers [subserverSeq % 3] + ".tiles.wmflabs.org/bw-mapnik/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.ThunderForestLandscape:
				url = "http://" + subservers [subserverSeq % 3] + ".tile.thunderforest.com/landscape/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.OpenTopoMap:
				url = "http://" + subservers [subserverSeq % 3] + ".tile.opentopomap.org/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.MapBoxSatellite:
				url = "http://" + subservers [subserverSeq % 3] + ".tiles.mapbox.com/v3/tmcw.map-j5fsp01s/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.Sputnik:
				url = "http://" + subservers [subserverSeq % 3] + ".tiles.maps.sputnik.ru/tiles/kmt2/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + ".png";
				break;
			case TILE_SERVER.AerisWeather:
																//https://maps[server].aerisapi.com/[client_id]_[client_key]/[type]/[zoom]/[x]/[y]/[offset].png
				url = "http://maps" + ((subserverSeq % 4) + 1).ToString () + ".aerisapi.com/" + _tileServerClientId + "_" + _tileServerAPIKey + "/" + _tileServerLayerTypes + "/" + ti.zoomLevel + "/" + ti.x + "/" + ti.y + "/" + _tileServerTimeOffset + ".png";
				break;
			case TILE_SERVER.Custom:
				StringBuilder sb = new StringBuilder (_tileServerCustomUrl);
				sb.Replace ("$n$", subservers [subserverSeq % 3]);
				sb.Replace ("$N$", subservers [subserverSeq % 3]);
				sb.Replace ("$X$", ti.x.ToString ());
				sb.Replace ("$x$", ti.x.ToString ());
				sb.Replace ("$Y$", ti.y.ToString ());
				sb.Replace ("$y$", ti.y.ToString ());
				sb.Replace ("$Z$", ti.zoomLevel.ToString ());
				sb.Replace ("$z$", ti.zoomLevel.ToString ());
				url = sb.ToString ();
				break;
			default:
				Debug.LogError ("Tile server not defined: " + tileServer.ToString ());
				url = "";
				break;
			}

			if (_tileServer != TILE_SERVER.Custom && _tileServerAPIKey != null && _tileServerAPIKey.Length > 0) {
				url += "?" + _tileServerAPIKey;
			}

			return url;

		}

	}

}