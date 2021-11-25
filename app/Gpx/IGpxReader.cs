using System;
using System.Threading.Tasks;

namespace Gpx
{
    public interface IGpxReader
    {
        GpxAttributes Attributes { get; }
        GpxMetadata Metadata { get; }
        GpxRoute Route { get; }
        GpxTrack Track { get; }
        GpxWayPoint WayPoint { get; }

        bool Read(out GpxObjectType type);
    }
}