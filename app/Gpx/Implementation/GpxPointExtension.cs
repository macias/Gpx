using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpx.Implementation
{
    internal static class GpxPointExtension
    {
        internal static IEnumerable<GpxPoint> ToGpxPoints<T>(this IEnumerable<T> points)
    where T : GpxPoint
        {
            foreach (T gpxPoint in points)
            {
                GpxPoint point = new GpxPoint
                {
                    Longitude = gpxPoint.Longitude,
                    Latitude = gpxPoint.Latitude,
                    Elevation = gpxPoint.Elevation,
                    Time = gpxPoint.Time
                };

                yield return point;
            }
        }

    }

}