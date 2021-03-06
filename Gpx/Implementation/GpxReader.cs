﻿using MathUnit;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Gpx.Implementation
{
    // this reader is around x6 faster than async version, not Task flying around, GC can rest
    internal sealed class GpxReader : IGpxReader,IDisposable
    {
        private readonly XmlReader reader;

        private GpxObjectType objectType;
        public GpxAttributes Attributes { get; private set; }
        public GpxMetadata Metadata { get; private set; }
        public GpxWayPoint WayPoint { get; private set; }
        public GpxRoute Route { get; private set; }
        public GpxTrack Track { get; private set; }

        public GpxReader(MemoryStream stream)
        {
            reader = XmlReader.Create(stream, new XmlReaderSettings() { Async = false });
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public bool Read(out GpxObjectType type)
        {
            bool result = doRead();
            type = this.objectType;
            return result;
        }

        private  bool doRead()
        {
            if (this.Attributes == null)
            {
                ReadHeader();
                return true;
            }

            if (objectType == GpxObjectType.None)
            {
                return false;
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "metadata":
                                Metadata =  ReadGpxMetadata();
                                objectType = GpxObjectType.Metadata;
                                return true;
                            case "wpt":
                                WayPoint =  ReadGpxWayPoint();
                                objectType = GpxObjectType.WayPoint;
                                return true;
                            case "rte":
                                Route =  ReadGpxRoute();
                                objectType = GpxObjectType.Route;
                                return true;
                            case "trk":
                                Track =  ReadGpxTrack();
                                objectType = GpxObjectType.Track;
                                return true;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != "gpx")
                            throw new FormatException(reader.Name);
                        objectType = GpxObjectType.None;
                        return false;
                }
            }

            objectType = GpxObjectType.None;
            return false;
        }

        private void ReadHeader()
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name != "gpx")
                            throw new FormatException(reader.Name);

                        Attributes = ReadGpxAttributes();
                        objectType = GpxObjectType.Attributes;
                        return;
                }
            }

            throw new FormatException();
        }

        private GpxAttributes ReadGpxAttributes()
        {
            GpxAttributes attributes = new GpxAttributes();

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "version":
                        attributes.Version = reader.Value;
                        break;
                    case "creator":
                        attributes.Creator = reader.Value;
                        break;
                }
            }

            return attributes;
        }

        private GpxMetadata ReadGpxMetadata()
        {
            GpxMetadata metadata = new GpxMetadata();
            if (reader.IsEmptyElement)
                return metadata;

            string elementName = reader.Name;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "name":
                                metadata.Name = ReadContentAsString();
                                break;
                            case "desc":
                                metadata.Description = ReadContentAsString();
                                break;
                            case "author":
                                metadata.Author = ReadGpxPerson();
                                break;
                            case "copyright":
                                metadata.Copyright =ReadGpxCopyright();
                                break;
                            case "link":
                                metadata.Link =  ReadGpxLink();
                                break;
                            case "time":
                                metadata.Time = ReadContentAsDateTime();
                                break;
                            case "keywords":
                                metadata.Keywords =  ReadContentAsString();
                                break;
                            case "bounds":
                                ReadGpxBounds();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return metadata;
                }
            }

            throw new FormatException(elementName);
        }

        private  GpxWayPoint ReadGpxWayPoint()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxWayPoint wayPoint = new GpxWayPoint();
            GetPointLocation(wayPoint);
            if (isEmptyElement)
                return wayPoint;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                 ReadWayPointExtensions(wayPoint);
                                break;
                            default:
                                if (! ProcessPointField(wayPoint))
                                     SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return wayPoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxRoute ReadGpxRoute()
        {
            GpxRoute route = new GpxRoute();
            if (reader.IsEmptyElement)
                return route;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "name":
                                route.Name =  ReadContentAsString();
                                break;
                            case "cmt":
                                route.Comment =  ReadContentAsString();
                                break;
                            case "desc":
                                route.Description =  ReadContentAsString();
                                break;
                            case "src":
                                route.Source =  ReadContentAsString();
                                break;
                            case "link":
                                route.Links.Add( ReadGpxLink());
                                break;
                            case "number":
                                route.Number =  ReadContentAsInt();
                                break;
                            case "type":
                                route.Type =  ReadContentAsString();
                                break;
                            case "rtept":
                                route.Add( ReadGpxRoutePoint());
                                break;
                            case "extensions":
                                 ReadRouteExtensions(route);
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return route;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxRoutePoint ReadGpxRoutePoint()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxRoutePoint routePoint = new GpxRoutePoint();
            GetPointLocation(routePoint);
            if (isEmptyElement)
                return routePoint;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                 ReadRoutePointExtensions(routePoint);
                                break;
                            default:
                                if (! ProcessPointField(routePoint))
                                     SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return routePoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrack ReadGpxTrack()
        {
            GpxTrack track = new GpxTrack();
            if (reader.IsEmptyElement)
                return track;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "name":
                                track.Name =  ReadContentAsString();
                                break;
                            case "cmt":
                                track.Comment =  ReadContentAsString();
                                break;
                            case "desc":
                                track.Description =  ReadContentAsString();
                                break;
                            case "src":
                                track.Source =  ReadContentAsString();
                                break;
                            case "link":
                                track.Links.Add( ReadGpxLink());
                                break;
                            case "number":
                                track.Number =  ReadContentAsInt();
                                break;
                            case "type":
                                track.Type = ReadContentAsString();
                                break;
                            case "trkseg":
                                track.Segments.Add( ReadGpxTrackSegment());
                                break;
                            case "extensions":
                                 ReadTrackExtensions(track);
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return track;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrackSegment ReadGpxTrackSegment()
        {
            GpxTrackSegment segment = new GpxTrackSegment();
            if (reader.IsEmptyElement)
                return segment;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "trkpt":
                                segment.Add( ReadGpxTrackPoint());
                                break;
                            case "extensions":
                                 ReadTrackSegmentExtensions();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return segment;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrackPoint ReadGpxTrackPoint()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxTrackPoint trackPoint = new GpxTrackPoint();
            GetPointLocation(trackPoint);
            if (isEmptyElement) return trackPoint;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                 ReadTrackPointExtensions(trackPoint);
                                break;
                            default:
                                if (! ProcessPointField(trackPoint))
                                     SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return trackPoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPerson ReadGpxPerson()
        {
            GpxPerson person = new GpxPerson();
            if (reader.IsEmptyElement)
                return person;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "name":
                                person.Name =  ReadContentAsString();
                                break;
                            case "email":
                                person.Email =  ReadGpxEmail();
                                break;
                            case "link":
                                person.Link =  ReadGpxLink();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return person;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxEmail ReadGpxEmail()
        {
            GpxEmail email = new GpxEmail();
            if (reader.IsEmptyElement)
                return email;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "id":
                                email.Id =  ReadContentAsString();
                                break;
                            case "domain":
                                email.Domain =  ReadContentAsString();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return email;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxLink ReadGpxLink()
        {
            GpxLink link = new GpxLink();

            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "href":
                        link.Href = reader.Value;
                        break;
                }
            }

            if (isEmptyElement)
                return link;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "text":
                                link.Text =  ReadContentAsString();
                                break;
                            case "type":
                                link.MimeType =  ReadContentAsString();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return link;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxCopyright ReadGpxCopyright()
        {
            GpxCopyright copyright = new GpxCopyright();

            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "author":
                        copyright.Author = reader.Value;
                        break;
                }
            }

            if (isEmptyElement)
                return copyright;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "year":
                                copyright.Year =  ReadContentAsInt();
                                break;
                            case "license":
                                copyright.Licence =  ReadContentAsString();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return copyright;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxBounds ReadGpxBounds()
        {
            if (!reader.IsEmptyElement)
                throw new FormatException(reader.Name);

            GpxBounds bounds = new GpxBounds();

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "minlat":
                        bounds.MinLatitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case "maxlat":
                        bounds.MaxLatitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case "minlon":
                        bounds.MinLongitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case "maxlon":
                        bounds.MaxLongitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                }
            }

            return bounds;
        }

        private void ReadWayPointExtensions(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE
                            || reader.NamespaceURI == GpxNamespaces.GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "WaypointExtension":
                                     ReadGarminWayPointExtensions(wayPoint);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                        if (reader.NamespaceURI == GpxNamespaces.DLG_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "level":
                                    wayPoint.Level =  ReadContentAsInt();
                                    break;
                                case "aliases":
                                     ReadWayPointAliases(wayPoint);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadRouteExtensions(GpxRoute route)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "RouteExtension":
                                     ReadGarminTrackOrRouteExtensions(route);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadRoutePointExtensions(GpxRoutePoint routePoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "RoutePointExtension":
                                     ReadGarminRoutePointExtensions(routePoint);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadTrackExtensions(GpxTrack track)
        {
            if (reader.IsEmptyElement) return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "TrackExtension":
                                     ReadGarminTrackOrRouteExtensions(track);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadTrackSegmentExtensions()
        {
             SkipElement();
        }

        private void ReadTrackPointExtensions(GpxTrackPoint trackPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE ||
                            reader.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE ||
                            reader.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "TrackPointExtension":
                                     ReadGarminTrackPointExtensions(trackPoint);
                                    break;
                                default:
                                     SkipElement();
                                    break;
                            }

                            break;
                        }

                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName) throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminWayPointExtensions(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Proximity":
                                wayPoint.Proximity =  ReadContentAsDouble();
                                break;
                            case "Temperature":
                                wayPoint.Temperature =  ReadContentAsDouble();
                                break;
                            case "Depth":
                                wayPoint.Depth =  ReadContentAsDouble();
                                break;
                            case "DisplayMode":
                                wayPoint.DisplayMode =  ReadContentAsString();
                                break;
                            case "Categories":
                                 ReadGarminCategories(wayPoint);
                                break;
                            case "Address":
                                wayPoint.Address =  ReadGarminGpxAddress();
                                break;
                            case "PhoneNumber":
                                wayPoint.Phones.Add( ReadGarminGpxPhone());
                                break;
                            case "Samples":
                                wayPoint.Samples =  ReadContentAsInt();
                                break;
                            case "Expiration":
                                wayPoint.Expiration =  ReadContentAsDateTime();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName) throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminTrackOrRouteExtensions(GpxTrackOrRoute trackOrRoute)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "DisplayColor":
                                trackOrRoute.DisplayColor = (GpxColor)Enum.Parse(typeof(GpxColor),
                                     ReadContentAsString(), false);
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminRoutePointExtensions(GpxRoutePoint routePoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "rpt":
                                routePoint.RoutePoints.Add( ReadGarminAutoRoutePoint());
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminTrackPointExtensions(GpxTrackPoint trackPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Temperature":
                            case "atemp":
                                trackPoint.Temperature =  ReadContentAsDouble();
                                break;
                            case "wtemp":
                                trackPoint.WaterTemperature =  ReadContentAsDouble();
                                break;
                            case "Depth":
                            case "depth":
                                trackPoint.Depth =  ReadContentAsDouble();
                                break;
                            case "hr":
                                trackPoint.HeartRate =  ReadContentAsInt();
                                break;
                            case "cad":
                                trackPoint.Cadence =  ReadContentAsInt();
                                break;
                            case "speed":
                                trackPoint.Speed =  ReadContentAsDouble();
                                break;
                            case "course":
                                trackPoint.Course =  ReadContentAsDouble();
                                break;
                            case "bearing":
                                trackPoint.Bearing =  ReadContentAsDouble();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminCategories(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Category":
                                wayPoint.Categories.Add( ReadContentAsString());
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadWayPointAliases(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "alias":
                                wayPoint.Aliases.Add( ReadContentAsString());
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName) throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPoint ReadGarminAutoRoutePoint()
        {
            GpxPoint point = new GpxPoint();

            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GetPointLocation(point);
            if (isEmptyElement)
                return point;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                         SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return point;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxAddress ReadGarminGpxAddress()
        {
            GpxAddress address = new GpxAddress();
            if (reader.IsEmptyElement)
                return address;

            string elementName = reader.Name;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "StreetAddress":

                                if (string.IsNullOrEmpty(address.StreetAddress))
                                {
                                    address.StreetAddress =  ReadContentAsString();
                                    break;
                                }

                                address.StreetAddress += " " +  ReadContentAsString();
                                break;

                            case "City":
                                address.City =  ReadContentAsString();
                                break;
                            case "State":
                                address.State =  ReadContentAsString();
                                break;
                            case "Country":
                                address.Country =  ReadContentAsString();
                                break;
                            case "PostalCode":
                                address.PostalCode =  ReadContentAsString();
                                break;
                            default:
                                 SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return address;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPhone ReadGarminGpxPhone()
        {
            return new GpxPhone
            {
                Category = reader.GetAttribute("Category", GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE),
                Number =  ReadContentAsString()
            };
        }

        private void SkipElement()
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;
            int depth = reader.Depth;

            while ( reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Depth == depth && reader.Name == elementName)
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void GetPointLocation(GpxPoint point)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "lat":
                        point.Latitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case "lon":
                        point.Longitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                }
            }
        }

        private bool ProcessPointField(GpxPoint point)
        {
            switch (reader.Name)
            {
                case "ele":
                    point.Elevation =  ReadContentAsDouble();
                    return true;
                case "time":
                    point.Time =  ReadContentAsDateTime();
                    return true;
                case "magvar":
                    point.MagneticVar =  ReadContentAsDouble();
                    return true;
                case "geoidheight":
                    point.GeoidHeight =  ReadContentAsDouble();
                    return true;
                case "name":
                    point.Name =  ReadContentAsString();
                    return true;
                case "cmt":
                    point.Comment =  ReadContentAsString();
                    return true;
                case "desc":
                    point.Description =  ReadContentAsString();
                    return true;
                case "src":
                    point.Source =  ReadContentAsString();
                    return true;
                case "link":
                    point.Links.Add( ReadGpxLink());
                    return true;
                case "sym":
                    point.Symbol =  ReadContentAsString();
                    return true;
                case "type":
                    point.Type =  ReadContentAsString();
                    return true;
                case "fix":
                    point.FixType =  ReadContentAsString();
                    return true;
                case "sat":
                    point.Satelites =  ReadContentAsInt();
                    return true;
                case "hdop":
                    point.Hdop =  ReadContentAsDouble();
                    return true;
                case "vdop":
                    point.Vdop =  ReadContentAsDouble();
                    return true;
                case "pdop":
                    point.Pdop =  ReadContentAsDouble();
                    return true;
                case "ageofdgpsdata":
                    point.AgeOfData =  ReadContentAsDouble();
                    return true;
                case "dgpsid":
                    point.DgpsId =  ReadContentAsInt();
                    return true;
            }

            return false;
        }

        private string ReadContentAsString()
        {
            if (reader.IsEmptyElement)
                throw new FormatException(reader.Name);

            string elementName = reader.Name;
            string result = string.Empty;

            while ( reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        result = reader.Value;
                        break;

                    case XmlNodeType.EndElement:
                        return result;

                    case XmlNodeType.Element:
                        throw new FormatException(elementName);
                }
            }

            throw new FormatException(elementName);
        }

        private int ReadContentAsInt()
        {
            string value =  ReadContentAsString();
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private double ReadContentAsDouble()
        {
            string value =  ReadContentAsString();
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        private DateTime ReadContentAsDateTime()
        {
            string value =  ReadContentAsString();
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}