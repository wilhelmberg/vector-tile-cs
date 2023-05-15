using Mapbox.VectorTile.Geometry;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;

using static System.FormattableString;


namespace Mapbox.VectorTile.ExtensionMethods {


	public static class VectorTileFeatureExtensions {


		/// <summary>
		/// >Geometry in LatLng coordinates instead of internal tile coordinates
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="zoom">Zoom level of the tile</param>
		/// <param name="tileColumn">Column of the tile (OSM tile schema)</param>
		/// <param name="tileRow">Row of the tile (OSM tile schema)</param>
		/// <returns></returns>
		public static List<List<LatLng>> GeometryAsWgs84(
			this VectorTileFeature feature
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, uint? clipBuffer = null
			) {

			List<List<LatLng>> geometryAsWgs84 = new();
			foreach (var part in feature.Geometry<long>(clipBuffer, 1.0f)) {
				geometryAsWgs84.Add(
					part.Select(g => g.ToLngLat(zoom, tileColumn, tileRow, feature.Layer.Extent)).ToList()
				);
			}

			return geometryAsWgs84;
		}


		public static string GeometryAsGeoJsonGeometry(
			this VectorTileFeature feature
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, uint? clipBuffer = null
		) {

			(string? geoJsonCoords, string? geomType) = GetCoordsAndType(feature, zoom, tileColumn, tileRow, clipBuffer);

			if (
				string.IsNullOrWhiteSpace(geoJsonCoords)
				|| string.IsNullOrWhiteSpace(geomType)
			) {
				return null;
			}

			return GeoJsonHelper.CreateGeometry(
				geomType
				, geoJsonCoords
			);
		}


		public static string? GeometryAsGeoJsonFeature(
			this VectorTileFeature feature
			, string layerName
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, uint? clipBuffer = null
			) {

			var keyValue = feature.GetProperties().Select(p => Invariant($@"""{p.Key}"":""{p.Value}"""));

			string geojsonProps = string.Format(
				NumberFormatInfo.InvariantInfo
				, @"{{""id"":{0},""lyr"":""{1}""{2}{3}}}"
				, feature.Id
				, layerName
				, keyValue.Any() ? "," : ""
				, string.Join(",", keyValue.ToArray())
			);


			(string? geoJsonCoords, string? geomType) = GetCoordsAndType(feature, zoom, tileColumn, tileRow, clipBuffer);

			if (
				string.IsNullOrWhiteSpace(geoJsonCoords)
				|| string.IsNullOrWhiteSpace(geomType)
			) {
				return null;
			}

			return GeoJsonHelper.CreateFeature(
				geomType
				, geoJsonCoords
				, geojsonProps
			);
		}


		private static (string? geoJsonCoords, string? geomType) GetCoordsAndType(
			VectorTileFeature feature
			, ulong zoom
			, ulong tileColumn
			, ulong tileRow
			, uint? clipBuffer = null
		) {

			string? geoJsonCoords = null;
			string? geomType = feature.GeometryType.Description();

			List<List<LatLng>> geomWgs84 = feature.GeometryAsWgs84(zoom, tileColumn, tileRow, clipBuffer);

			//multipart
			if (geomWgs84.Count > 1) {
				switch (feature.GeometryType) {
					case GeomType.POINT:
						geomType = "MultiPoint";

						geoJsonCoords = string.Join(
							","
							, geomWgs84
								.SelectMany((List<LatLng> g) => g)
								.Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
						);
						break;
					case GeomType.LINESTRING:
						geomType = "MultiLineString";
						List<string> parts = new();
						foreach (var part in geomWgs84) {
							parts.Add(
								"[" + string.Join(
									","
									, part.Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
								) + "]");
						}
						geoJsonCoords = string.Join(",", parts.ToArray());
						break;
					case GeomType.POLYGON:
						geomType = "MultiPolygon";
						List<string> partsMP = new();
						foreach (var part in geomWgs84) {
							partsMP.Add(
								"[" + string.Join(
									","
									, part.Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
								) + "]"
							);
						}
						geoJsonCoords = "[" + string.Join(",", partsMP.ToArray()) + "]";
						break;
					default:
						break;
				}
			} else if (geomWgs84.Count == 1) { //singlepart
				switch (feature.GeometryType) {
					case GeomType.POINT:
						geoJsonCoords = string.Format(NumberFormatInfo.InvariantInfo, "{0},{1}", geomWgs84[0][0].Lng, geomWgs84[0][0].Lat);
						break;
					case GeomType.LINESTRING:
						geoJsonCoords = string.Join(
							","
							, geomWgs84[0].Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
						);
						break;
					case GeomType.POLYGON:
						geoJsonCoords = "[" + string.Join(
							","
							, geomWgs84[0].Select(g => string.Format(NumberFormatInfo.InvariantInfo, "[{0},{1}]", g.Lng, g.Lat)).ToArray()
						) + "]";
						break;
					default:
						break;
				}
			} else {
				//no geometry
			}


			return (geoJsonCoords, geomType);
		}



	}
}
