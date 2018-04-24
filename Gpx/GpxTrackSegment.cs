// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

using System;
using System.Linq;

namespace Gpx
{
    public sealed class GpxTrackSegment
    {
        private readonly GpxPointCollection<GpxTrackPoint> trackPoints = new GpxPointCollection<GpxTrackPoint>();

        public GpxPointCollection<GpxTrackPoint> TrackPoints
        {
            get { return trackPoints; }
        }

        public override string ToString()
        {
            return String.Join(" ", TrackPoints.Select(it => "(" + it.ToString() + ")"));
        }
    }

}