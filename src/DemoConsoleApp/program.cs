using Mapbox.VectorTile.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;


namespace Mapbox.VectorTile {


	public class DemoConsoleApp {


		public static int Main(string[] args) {

			string vtIn = string.Empty;
			uint? clipBuffer = null;
			bool outGeoJson = false;
			ulong? zoom = null;
			ulong? tileCol = null;
			ulong? tileRow = null;
			bool validate = true;

			for (int i = 0; i < args.Length; i++) {
				string argLow = args[i].ToLower();
				if (argLow.Contains("vt:")) {
					vtIn = argLow.Replace("vt:", "");
				} else if (argLow.Contains("clip:")) {
					clipBuffer = Convert.ToUInt32(argLow.Replace("clip:", ""));
				} else if (argLow.Contains("out:")) {
					outGeoJson = argLow.Replace("out:", "").Equals("geojson");
				} else if (argLow.Contains("tileid:")) {
					ParseArg(argLow.Replace("tileid:", ""), out zoom, out tileCol, out tileRow);
				} else if (argLow.Contains("novalidate")) {
					validate = false;
				}
			}

			if (!File.Exists(vtIn)) {
				Console.WriteLine($"file [{vtIn}] not found");
				Usage();
				return 1;
			}

			// z-x-y weren't passed via parameters, try to get them from file name
			if (!zoom.HasValue || !tileCol.HasValue || !tileRow.HasValue) {
				if (!ParseArg(Path.GetFileName(vtIn), out zoom, out tileCol, out tileRow)) {
					Usage();
					return 1;
				}
			}

			var bufferedData = File.ReadAllBytes(vtIn);

			VectorTile tile = new(bufferedData, validate);

			if (outGeoJson) {
				Console.WriteLine(tile.ToGeoJson(zoom.Value, tileCol.Value, tileRow.Value, clipBuffer));
			} else {
				foreach (string lyrName in tile.LayerNames()) {
					VectorTileLayer lyr = tile.GetLayer(lyrName);
					Console.WriteLine(string.Format("------------ LAYER: {0} ---------", lyrName));
					//if (lyrName != "building") { continue; }
					int featCnt = lyr.FeatureCount();
					for (int i = 0; i < featCnt; i++) {
						VectorTileFeature feat = lyr.GetFeature(i, clipBuffer);
						Console.WriteLine(string.Format("feature {0}: {1}", i, feat.GeometryType));
						Dictionary<string, object> props = feat.GetProperties();
						foreach (var prop in props) {
							Console.WriteLine(string.Format("   {0}\t : ({1}) {2}", prop.Key, prop.Value.GetType(), prop.Value));
						}
					}
				}
			}

			return 0;
		}

		private static void Usage() {

			Console.WriteLine("");
			Console.WriteLine("DemoConsoleApp.exe vt:<tile.mvt> <other parameters>");
			Console.WriteLine("");
			Console.WriteLine("- vt:<path/to/vector/tile.mvt> or vt:<path/to/<z>-<x>-<y>.tile.mvt>");
			Console.WriteLine("- clip:<buffer>           to clip geometries extending beyond the tile border");
			Console.WriteLine("- out:<geojson|metadata>  to output either GeoJson or some metadata");
			Console.WriteLine("- tileid:<z>-<x>-<y>      to pass tile id if not contained within the file name");
			Console.WriteLine("- novalidate              ignore tile errors");
			Console.WriteLine("");
			Console.WriteLine("");
		}

		private static bool ParseArg(string fileName, out ulong? zoom, out ulong? tileCol, out ulong? tileRow) {
			zoom = null;
			tileCol = null;
			tileRow = null;

			string zxyTxt = fileName.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
			string[] zxy = zxyTxt.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (zxy.Length != 3) {
				Console.WriteLine("invalid zoom, tileCol or tileRow [{0}]", zxyTxt);
				return false;
			}

			if (!ulong.TryParse(zxy[0], out ulong z)) {
				Console.WriteLine($"could not parse zoom: {zxy[0]}");
				return false;
			}
			zoom = z;

			if (!ulong.TryParse(zxy[1], out ulong x)) {
				Console.WriteLine($"could not parse tileCol: {zxy[1]}");
				return false;
			}
			tileCol = x;

			if (!ulong.TryParse(zxy[2], out ulong y)) {
				Console.WriteLine($"could not parse tileRow: {zxy[2]}");
				return false;
			}
			tileRow = y;

			return true;
		}



	}
}
