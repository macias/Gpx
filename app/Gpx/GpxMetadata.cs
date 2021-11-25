using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpx
{
    public class GpxMetadata 
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GpxPerson Author { get; set; }
        public GpxCopyright Copyright { get; set; }
        public GpxLink Link { get; set; }
        public DateTime? Time { get; set; }
        public string Keywords { get; set; }
        public GpxBounds Bounds { get; set; }
    }
}