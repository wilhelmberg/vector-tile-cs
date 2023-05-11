using Mapbox.VectorTile.Geometry;

using System.Collections.Generic;

using System.Linq;

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



	}
}
