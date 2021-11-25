using System;
using System.Threading.Tasks;

namespace Gpx
{
    public interface IGpxAsyncReader 
    {
        GpxAttributes Attributes { get; }
        GpxMetadata Metadata { get; }
        GpxObjectType ObjectType { get; }
        GpxRoute Route { get; }
        GpxTrack Track { get; }
        GpxWayPoint WayPoint { get; }

        Task<bool> ReadAsync();
    }
}