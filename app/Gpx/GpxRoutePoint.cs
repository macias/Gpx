using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpx
{
    public sealed class GpxRoutePoint : GpxPoint
    {
        // GARMIN_EXTENSIONS

        public IList<GpxPoint> RoutePoints { get; }

        public bool HasExtensions
        {
            get { return RoutePoints.Count != 0; }
        }

        public GpxRoutePoint()
        {
            this.RoutePoints = new List<GpxPoint>();
        }
    }

}