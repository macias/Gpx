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
    public class GpxPoint : GeoPoint
    {
        public double? Elevation { get; set; }
        public DateTime? Time { get; set; }

        public double? MagneticVar { get; set; }

        public double? GeoidHeight { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public IList<GpxLink> Links { get; }

        public string Symbol { get; set; }

        public string Type { get; set; }

        public string FixType { get; set; }

        public int? Satelites { get; set; }

        public double? Hdop { get; set; }

        public double? Vdop { get; set; }

        public double? Pdop { get; set; }

        public double? AgeOfData { get; set; }

        public int? DgpsId { get; set; }

        public GpxLink HttpLink
        {
            get
            {
                return Links.Where(l => l != null && l.Uri != null && l.Uri.Scheme == Uri.UriSchemeHttp).FirstOrDefault();
            }
        }

        public GpxLink EmailLink
        {
            get
            {
                return Links.Where(l => l != null && l.Uri != null && l.Uri.Scheme == Uri.UriSchemeMailto).FirstOrDefault();
            }
        }


        public GpxPoint()
        {
            this.Links = new List<GpxLink>();
        }

     }

}