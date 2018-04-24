// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

namespace Gpx
{
    public class GeoPoint : IGeoPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return Latitude.ToString() + "," + Longitude.ToString();
        }
        public string ToString(string format)
        {
            return Latitude.ToString(format) + "," + Longitude.ToString(format);
        }
    }
}