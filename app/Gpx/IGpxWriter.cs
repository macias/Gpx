using System;

namespace Gpx
{
    public interface IGpxWriter 
    {
        void WriteMetadata(GpxMetadata metadata);
        void WriteRoute(GpxRoute route);
        void WriteTrack(GpxTrack track);
        void WriteWayPoint(GpxWayPoint wayPoint);
    }
}