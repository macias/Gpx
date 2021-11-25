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

        internal void Add(GpxTrackPoint point)
        {
            this.points.Add(point);
        }

        public override string ToString()
        {
            return String.Join(" ", TrackPoints.Select(it => "(" + it.ToString() + ")"));
        }
    }

}