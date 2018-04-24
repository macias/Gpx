using System;

namespace Gpx
{
    public interface IGeoPoint
    {
        double Latitude { get; set; } // degrees
        double Longitude { get; set; }

        string ToString(string format);
    }
}