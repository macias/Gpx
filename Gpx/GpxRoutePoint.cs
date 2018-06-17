// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

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