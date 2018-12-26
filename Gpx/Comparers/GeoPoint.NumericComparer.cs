using Gpx;
using System.Collections.Generic;

namespace Gpx.Comparers
{
    public sealed class GeoPointNumericComparer : IEqualityComparer<IGeoPoint>
    {
        public static IEqualityComparer<IGeoPoint> Default { get; } = new GeoPointNumericComparer();

        private GeoPointNumericComparer()
        {

        }

        public bool Equals(IGeoPoint x, IGeoPoint y)
        {
            return x.Latitude.Equals(y.Latitude) && x.Longitude.Equals(y.Longitude);
        }

        public int GetHashCode(IGeoPoint obj)
        {
            return obj.Latitude.GetHashCode() ^ obj.Longitude.GetHashCode();
        }
    }
}