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
    public static class GpxNamespaces
    {
        public const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
        public const string GARMIN_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";
        public const string GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v1";
        public const string GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v2";
        public const string GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/WaypointExtension/v1";
        public const string DLG_EXTENSIONS_NAMESPACE = "http://dlg.krakow.pl/gpx/extensions/v1";
    }

    public class GpxAttributes
    {
        public string Version { get; set; }
        public string Creator { get; set; }
    }

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

    public abstract class GpxTrackOrRoute
    {
        private readonly List<GpxLink> Links_ = new List<GpxLink>(0);

        public string Name { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int? Number { get; set; }
        public string Type { get; set; }

        public IList<GpxLink> Links
        {
            get { return Links_; }
        }

        // GARMIN_EXTENSIONS

        public GpxColor? DisplayColor { get; set; }

        public bool HasExtensions
        {
            get { return DisplayColor != null; }
        }

        public abstract IEnumerable<GpxPoint> ToGpxPoints();
    }

    public class GpxRoute : GpxTrackOrRoute
    {
        private readonly List<GpxRoutePoint> RoutePoints_ = new List<GpxRoutePoint>();

        public IReadOnlyList<GpxRoutePoint> RoutePoints
        {
            get { return RoutePoints_; }
        }

        public override IEnumerable<GpxPoint> ToGpxPoints()
        {
            var points = new List<GpxPoint>();

            foreach (GpxRoutePoint routePoint in RoutePoints_)
            {
                points.Add(routePoint);

                foreach (GpxPoint gpxPoint in routePoint.RoutePoints)
                {
                    points.Add(gpxPoint);
                }
            }

            return points;
        }

        internal void Add(GpxRoutePoint point)
        {
            this.RoutePoints_.Add(point);
        }
    }

    public class GpxLink
    {
        public string Href { get; set; }
        public string Text { get; set; }
        public string MimeType { get; set; }

        public Uri Uri
        {
            get
            {
                Uri result;
                return Uri.TryCreate(Href, UriKind.Absolute, out result) ? result : null;
            }
        }
    }

    public class GpxEmail
    {
        public string Id { get; set; }
        public string Domain { get; set; }
    }

    public class GpxAddress
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class GpxPhone
    {
        public string Number { get; set; }
        public string Category { get; set; }
    }

    public class GpxPerson
    {
        public string Name { get; set; }
        public GpxEmail Email { get; set; }
        public GpxLink Link { get; set; }
    }

}