using Mapbox.VectorTile.Geometry;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Mapbox.VectorTile.ExtensionMethods {


	public static class GeoJsonHelper {

		public static string FeatureCollectionTemplate = @"{{""type"":""FeatureCollection"",""features"":[{0}]}}";
		public static string FeatureTemplate = @"{{""type"":""Feature"",""geometry"":{{""type"":""{0}"",""coordinates"":[{1}]}},""properties"":{2}}}";
		public static string GeometryTemplate = @"{{""type"":""{0}"",""coordinates"":[{1}]}}";


		public static string CreateGeometry(
			string geometryType
			, string coordinates
		) {
			return string.Format(
				NumberFormatInfo.InvariantInfo
				, GeometryTemplate
				, geometryType
				, coordinates
				);
		}


		public static string CreateFeature(
			string geometryType
			, string coordinates
			, string properties
		) {

			return string.Format(
				NumberFormatInfo.InvariantInfo
				, FeatureTemplate
				, geometryType
				, coordinates
				, properties
			);
		}


		public static string CreateFeatureCollection(
			List<string> features
		) {
			return string.Format(
				NumberFormatInfo.InvariantInfo
				, FeatureCollectionTemplate
				, string.Join(",", features)
			);
		}

	}
}
