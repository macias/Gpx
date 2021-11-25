using MathUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gpx
{
    public class GpxPoint : IGpxPoint 
    {
        public Angle Latitude { get; set; }
        public Angle Longitude { get; set; }

        public double? Elevation { get; set; }
        public DateTimeOffset? Time { get; set; }

        public double? MagneticVar { get; set; }

        public double? GeoidHeight { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public IList<GpxLink> Links { get; }
        IEnumerable<GpxLink> IGpxPoint.Links => this.Links;

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

        public override string ToString()
        {
            return Latitude.ToString(CultureInfo.InvariantCulture) + "," + Longitude.ToString(CultureInfo.InvariantCulture);
        }

    }

}