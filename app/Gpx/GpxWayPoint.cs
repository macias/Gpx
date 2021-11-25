using System;
using System.Collections.Generic;
using System.Linq;

namespace Gpx
{
    public sealed class GpxWayPoint : GpxPoint
    {
        // GARMIN_EXTENSIONS, GARMIN_WAYPOINT_EXTENSIONS

        public double? Proximity { get; set; }

        public double? Temperature { get; set; }

        public double? Depth { get; set; }

        public string DisplayMode { get; set; }

        public IList<string> Categories { get; }

        public GpxAddress Address { get; set; }

        public IList<GpxPhone> Phones { get; }

        // GARMIN_WAYPOINT_EXTENSIONS

        public int? Samples { get; set; }

        public DateTime? Expiration { get; set; }

        // DLG_EXTENSIONS

        public int? Level { get; set; }

        public IList<string> Aliases { get; }

        public bool HasGarminExtensions
        {
            get
            {
                return Proximity != null || Temperature != null || Depth != null ||
                    DisplayMode != null || Address != null ||
                    Categories.Count != 0 || Phones.Count != 0;
            }
        }

        public bool HasGarminWaypointExtensions
        {
            get { return Samples != null || Expiration != null; }
        }

        public bool HasDlgExtensions
        {
            get { return Level != null || Aliases.Count != 0; }
        }

        public bool HasExtensions
        {
            get { return HasGarminExtensions || HasGarminWaypointExtensions || HasDlgExtensions; }
        }

        public GpxWayPoint()
        {
            this.Phones = new List<GpxPhone>();
            this.Categories = new List<string>();
            this.Aliases = new List<string>();
        }
    }

  
}