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
    public sealed class GpxTrack : GpxTrackOrRoute
    {
        private readonly List<GpxTrackSegment> segments = new List<GpxTrackSegment>(1);

        public IList<GpxTrackSegment> Segments
        {
            get { return segments; }
        }

        public override IEnumerable<GpxPoint> ToGpxPoints()
        {
            var points = new List<GpxPoint>();

            foreach (GpxTrackSegment segment in segments)
            {
                IEnumerable<GpxPoint> segmentPoints = segment.TrackPoints.ToGpxPoints();

                foreach (GpxPoint point in segmentPoints)
                {
                    points.Add(point);
                }
            }

            return points;
        }

        public override string ToString()
        {
            return String.Join(" ",Segments.Select(it => "["+it.ToString()+"]"));
        }
    }
}