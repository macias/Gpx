using MathUnit;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Gpx.Implementation
{
    // this reader is around x6 faster than async version, not Task flying around, GC can rest
    internal sealed class GpxReader<TTrackPoint> : IGpxReader, IDisposable
                where TTrackPoint : GpxTrackPoint, new()
    {
        private readonly XmlReader reader;
        private readonly IGpxTrackPointReader<TTrackPoint> trackPointReader;
        private GpxObjectType objectType;
        public GpxAttributes Attributes { get; private set; }
        public GpxMetadata Metadata { get; private set; }
        public GpxWayPoint WayPoint { get; private set; }
        public GpxRoute Route { get; private set; }
        public GpxTrack Track { get; private set; }

        public GpxReader(MemoryStream stream, IGpxTrackPointReader<TTrackPoint> trackPointReader)
        {
            reader = XmlReader.Create(stream, new XmlReaderSettings() { Async = false });
            this.trackPointReader = trackPointReader;
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

        private bool doRead()
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
                                Metadata = ReadGpxMetadata();
                                objectType = GpxObjectType.Metadata;
                                return true;
                            case GpxSymbol.Waypoint:
                                WayPoint = ReadGpxWayPoint();
                                objectType = GpxObjectType.WayPoint;
                                return true;
                            case "rte":
                                Route = ReadGpxRoute();
                                objectType = GpxObjectType.Route;
                                return true;
                            case GpxSymbol.Track:
                                Track = ReadGpxTrack();
                                objectType = GpxObjectType.Track;
                                return true;
                            default:
                                reader.SkipElement();
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
                            case GpxSymbol.Name:
                                metadata.Name = reader.ReadElementContentAsString();
                                break;
                            case "desc":
                                metadata.Description = reader.ReadElementContentAsString();
                                break;
                            case "author":
                                metadata.Author = ReadGpxPerson();
                                break;
                            case "copyright":
                                metadata.Copyright = ReadGpxCopyright();
                                break;
                            case "link":
                                metadata.Link = ReadGpxLink();
                                break;
                            case GpxSymbol.Time:
                                metadata.Time = reader.ReadElementContentAsDateTime();
                                break;
                            case "keywords":
                                metadata.Keywords = reader.ReadElementContentAsString();
                                break;
                            case "bounds":
                                ReadGpxBounds();
                                break;
                            default:
                                reader.SkipElement();
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

        private GpxWayPoint ReadGpxWayPoint()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxWayPoint wayPoint = new GpxWayPoint();
            GetPointLocation(wayPoint);
            if (isEmptyElement)
                return wayPoint;

            while (reader.Read())
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
                                if (!ProcessPointField(wayPoint))
                                    reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                route.Name = reader.ReadElementContentAsString();
                                break;
                            case GpxSymbol.Comment:
                                route.Comment = reader.ReadElementContentAsString();
                                break;
                            case "desc":
                                route.Description = reader.ReadElementContentAsString();
                                break;
                            case "src":
                                route.Source = reader.ReadElementContentAsString();
                                break;
                            case "link":
                                route.Links.Add(ReadGpxLink());
                                break;
                            case "number":
                                route.Number = reader.ReadElementContentAsInt();
                                break;
                            case "type":
                                route.Type = reader.ReadElementContentAsString();
                                break;
                            case "rtept":
                                route.Add(ReadGpxRoutePoint());
                                break;
                            case "extensions":
                                ReadRouteExtensions(route);
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
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
                                if (!ProcessPointField(routePoint))
                                    reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                track.Name = reader.ReadElementContentAsString();
                                break;
                            case GpxSymbol.Comment:
                                track.Comment = reader.ReadElementContentAsString();
                                break;
                            case "desc":
                                track.Description = reader.ReadElementContentAsString();
                                break;
                            case "src":
                                track.Source = reader.ReadElementContentAsString();
                                break;
                            case "link":
                                track.Links.Add(ReadGpxLink());
                                break;
                            case "number":
                                track.Number = reader.ReadElementContentAsInt();
                                break;
                            case "type":
                                track.Type = reader.ReadElementContentAsString();
                                break;
                            case GpxSymbol.TrackSegment:
                                track.Segments.Add(ReadGpxTrackSegment());
                                break;
                            case "extensions":
                                ReadTrackExtensions(track);
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.TrackPoint:
                                segment.Add(ReadGpxTrackPoint());
                                break;
                            case "extensions":
                                ReadTrackSegmentExtensions();
                                break;
                            default:
                                reader.SkipElement();
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

            var trackPoint = new TTrackPoint();
            GetPointLocation(trackPoint);
            if (isEmptyElement)
                return trackPoint;

            while (reader.Read())
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
                                if (!ProcessPointField(trackPoint)
                                    &&
                                    !trackPointReader.TryReadBody(reader, trackPoint))
                                    reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                person.Name = reader.ReadElementContentAsString();
                                break;
                            case "email":
                                person.Email = ReadGpxEmail();
                                break;
                            case "link":
                                person.Link = ReadGpxLink();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "id":
                                email.Id = reader.ReadElementContentAsString();
                                break;
                            case "domain":
                                email.Domain = reader.ReadElementContentAsString();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "text":
                                link.Text = reader.ReadElementContentAsString();
                                break;
                            case "type":
                                link.MimeType = reader.ReadElementContentAsString();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "year":
                                copyright.Year = reader.ReadElementContentAsInt();
                                break;
                            case "license":
                                copyright.Licence = reader.ReadElementContentAsString();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
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
                                    reader.SkipElement();
                                    break;
                            }

                            break;
                        }

                        if (reader.NamespaceURI == GpxNamespaces.DLG_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "level":
                                    wayPoint.Level = reader.ReadElementContentAsInt();
                                    break;
                                case "aliases":
                                    ReadWayPointAliases(wayPoint);
                                    break;
                                default:
                                    reader.SkipElement();
                                    break;
                            }

                            break;
                        }

                        reader.SkipElement();
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

            while (reader.Read())
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
                                    reader.SkipElement();
                                    break;
                            }

                            break;
                        }

                        reader.SkipElement();
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

            while (reader.Read())
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
                                    reader.SkipElement();
                                    break;
                            }

                            break;
                        }

                        reader.SkipElement();
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

            while (reader.Read())
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
                                    reader.SkipElement();
                                    break;
                            }

                            break;
                        }

                        reader.SkipElement();
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
            reader.SkipElement();
        }

        private void ReadTrackPointExtensions(TTrackPoint trackPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        {

                            if ((reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE
                                || reader.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE
                                || reader.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE)
                                && reader.LocalName == "TrackPointExtension")
                            {
                                ReadGarminTrackPointExtensions(trackPoint);
                            }
                            else
                            {
                                if (!trackPointReader.TryReadExtension(reader, trackPoint))
                                    reader.SkipElement();
                            }

                            break;
                        }
                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case GpxSymbol.Proximity:
                                wayPoint.Proximity = reader.ReadElementContentAsDouble();
                                break;
                            case "Temperature":
                                wayPoint.Temperature = reader.ReadElementContentAsDouble();
                                break;
                            case "Depth":
                                wayPoint.Depth = reader.ReadElementContentAsDouble();
                                break;
                            case "DisplayMode":
                                wayPoint.DisplayMode = reader.ReadElementContentAsString();
                                break;
                            case "Categories":
                                ReadGarminCategories(wayPoint);
                                break;
                            case "Address":
                                wayPoint.Address = ReadGarminGpxAddress();
                                break;
                            case "PhoneNumber":
                                wayPoint.Phones.Add(ReadGarminGpxPhone());
                                break;
                            case "Samples":
                                wayPoint.Samples = reader.ReadElementContentAsInt();
                                break;
                            case "Expiration":
                                wayPoint.Expiration = reader.ReadElementContentAsDateTime();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "DisplayColor":
                                trackOrRoute.DisplayColor = (GpxColor)Enum.Parse(typeof(GpxColor),
                                     reader.ReadElementContentAsString(), false);
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "rpt":
                                routePoint.RoutePoints.Add(ReadGarminAutoRoutePoint());
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Temperature":
                            case "atemp":
                                trackPoint.Temperature = reader.ReadElementContentAsDouble();
                                break;
                            case "wtemp":
                                trackPoint.WaterTemperature = reader.ReadElementContentAsDouble();
                                break;
                            case "Depth":
                            case "depth":
                                trackPoint.Depth = reader.ReadElementContentAsDouble();
                                break;
                            case "hr":
                                trackPoint.HeartRate = reader.ReadElementContentAsInt();
                                break;
                            case "cad":
                                trackPoint.Cadence = reader.ReadElementContentAsInt();
                                break;
                            case "speed":
                                trackPoint.Speed = reader.ReadElementContentAsDouble();
                                break;
                            case "course":
                                trackPoint.Course = reader.ReadElementContentAsDouble();
                                break;
                            case "bearing":
                                trackPoint.Bearing = reader.ReadElementContentAsDouble();
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Category":
                                wayPoint.Categories.Add(reader.ReadElementContentAsString());
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "alias":
                                wayPoint.Aliases.Add(reader.ReadElementContentAsString());
                                break;
                            default:
                                reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        reader.SkipElement();
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

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "StreetAddress":

                                if (string.IsNullOrEmpty(address.StreetAddress))
                                {
                                    address.StreetAddress = reader.ReadElementContentAsString();
                                    break;
                                }

                                address.StreetAddress += " " + reader.ReadElementContentAsString();
                                break;

                            case "City":
                                address.City = reader.ReadElementContentAsString();
                                break;
                            case "State":
                                address.State = reader.ReadElementContentAsString();
                                break;
                            case "Country":
                                address.Country = reader.ReadElementContentAsString();
                                break;
                            case "PostalCode":
                                address.PostalCode = reader.ReadElementContentAsString();
                                break;
                            default:
                                reader.SkipElement();
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
                Number = reader.ReadElementContentAsString()
            };
        }


        private void GetPointLocation(GpxPoint point)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case GpxSymbol.Latitude:
                        point.Latitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case GpxSymbol.Longitude:
                        point.Longitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                }
            }
        }

        private bool ProcessPointField(GpxPoint point)
        {
            switch (reader.Name)
            {
                case GpxSymbol.Elevation:
                    point.Elevation = reader.ReadElementContentAsDouble();
                    return true;
                case GpxSymbol.Time:
                    point.Time = reader.ReadElementContentAsDateTime();
                    return true;
                case "magvar":
                    point.MagneticVar = reader.ReadElementContentAsDouble();
                    return true;
                case "geoidheight":
                    point.GeoidHeight = reader.ReadElementContentAsDouble();
                    return true;
                case GpxSymbol.Name:
                    point.Name = reader.ReadElementContentAsString();
                    return true;
                case GpxSymbol.Comment:
                    point.Comment = reader.ReadElementContentAsString();
                    return true;
                case "desc":
                    point.Description = reader.ReadElementContentAsString();
                    return true;
                case "src":
                    point.Source = reader.ReadElementContentAsString();
                    return true;
                case "link":
                    point.Links.Add(ReadGpxLink());
                    return true;
                case "sym":
                    point.Symbol = reader.ReadElementContentAsString();
                    return true;
                case "type":
                    point.Type = reader.ReadElementContentAsString();
                    return true;
                case "fix":
                    point.FixType = reader.ReadElementContentAsString();
                    return true;
                case "sat":
                    point.Satelites = reader.ReadElementContentAsInt();
                    return true;
                case "hdop":
                    point.Hdop = reader.ReadElementContentAsDouble();
                    return true;
                case "vdop":
                    point.Vdop = reader.ReadElementContentAsDouble();
                    return true;
                case "pdop":
                    point.Pdop = reader.ReadElementContentAsDouble();
                    return true;
                case "ageofdgpsdata":
                    point.AgeOfData = reader.ReadElementContentAsDouble();
                    return true;
                case "dgpsid":
                    point.DgpsId = reader.ReadElementContentAsInt();
                    return true;
            }

            return false;
        }

    }
}