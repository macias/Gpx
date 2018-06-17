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
    public sealed class GpxTrackSegment
    {
        private readonly List<GpxTrackPoint> points;
        public IReadOnlyList<GpxTrackPoint> TrackPoints => this.points;

        public GpxTrackSegment(IEnumerable<GpxTrackPoint> points)
        {
            this.points = points.ToList();
        }
        public GpxTrackSegment() : this(Enumerable.Empty<GpxTrackPoint>())
        {

        }
        public override string ToString()
        {
            return String.Join(" ", TrackPoints.Select(it => "(" + it.ToString() + ")"));
        }

        internal void Add(GpxTrackPoint point)
        {
            this.points.Add(point);
        }
    }

}