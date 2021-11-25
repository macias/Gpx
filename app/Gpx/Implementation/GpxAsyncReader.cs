using MathUnit;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Gpx.Implementation
{
    internal sealed class GpxAsyncReader : IGpxAsyncReader, IDisposable
    {
        private readonly XmlReader reader;

        public GpxObjectType ObjectType { get; private set; }
        public GpxAttributes Attributes { get; private set; }
        public GpxMetadata Metadata { get; private set; }
        public GpxWayPoint WayPoint { get; private set; }
        public GpxRoute Route { get; private set; }
        public GpxTrack Track { get; private set; }

        public GpxAsyncReader(Stream stream)
        {
            reader = XmlReader.Create(stream, new XmlReaderSettings() { Async = true });
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public async Task<bool> ReadAsync()
        {
            if (this.Attributes == null)
            {
                await ReadHeaderAsync().ConfigureAwait(false);
                return true;
            }

            if (ObjectType == GpxObjectType.None)
                return false;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "metadata":
                                Metadata = await ReadGpxMetadataAsync().ConfigureAwait(false);
                                ObjectType = GpxObjectType.Metadata;
                                return true;
                            case GpxSymbol.Waypoint:
                                WayPoint = await ReadGpxWayPointAsync().ConfigureAwait(false);
                                ObjectType = GpxObjectType.WayPoint;
                                return true;
                            case "rte":
                                Route = await ReadGpxRouteAsync().ConfigureAwait(false);
                                ObjectType = GpxObjectType.Route;
                                return true;
                            case GpxSymbol.Track:
                                Track = await ReadGpxTrackAsync().ConfigureAwait(false);
                                ObjectType = GpxObjectType.Track;
                                return true;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != "gpx")
                            throw new FormatException(reader.Name);
                        ObjectType = GpxObjectType.None;
                        return false;
                }
            }

            ObjectType = GpxObjectType.None;
            return false;
        }

        private async Task ReadHeaderAsync()
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name != "gpx")
                            throw new FormatException(reader.Name);

                        Attributes = ReadGpxAttributes();
                        ObjectType = GpxObjectType.Attributes;
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

        private async Task<GpxMetadata> ReadGpxMetadataAsync()
        {
            GpxMetadata metadata = new GpxMetadata();
            if (reader.IsEmptyElement)
                return metadata;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                metadata.Name = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "desc":
                                metadata.Description = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "author":
                                metadata.Author = await ReadGpxPersonAsync().ConfigureAwait(false);
                                break;
                            case "copyright":
                                metadata.Copyright = await ReadGpxCopyrightAsync().ConfigureAwait(false);
                                break;
                            case "link":
                                metadata.Link = await ReadGpxLinkAsync().ConfigureAwait(false);
                                break;
                            case GpxSymbol.Time:
                                metadata.Time = await ReadContentAsDateTimeAsync().ConfigureAwait(false);
                                break;
                            case "keywords":
                                metadata.Keywords = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "bounds":
                                ReadGpxBounds();
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxWayPoint> ReadGpxWayPointAsync()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxWayPoint wayPoint = new GpxWayPoint();
            GetPointLocation(wayPoint);
            if (isEmptyElement)
                return wayPoint;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                await ReadWayPointExtensionsAsync(wayPoint).ConfigureAwait(false);
                                break;
                            default:
                                if (!await ProcessPointFieldAsync(wayPoint).ConfigureAwait(false))
                                    await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxRoute> ReadGpxRouteAsync()
        {
            GpxRoute route = new GpxRoute();
            if (reader.IsEmptyElement)
                return route;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                route.Name = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case GpxSymbol.Comment:
                                route.Comment = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "desc":
                                route.Description = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "src":
                                route.Source = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "link":
                                route.Links.Add(await ReadGpxLinkAsync().ConfigureAwait(false));
                                break;
                            case "number":
                                route.Number = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "type":
                                route.Type = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "rtept":
                                route.Add(await ReadGpxRoutePointAsync().ConfigureAwait(false));
                                break;
                            case "extensions":
                                await ReadRouteExtensionsAsync(route).ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxRoutePoint> ReadGpxRoutePointAsync()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxRoutePoint routePoint = new GpxRoutePoint();
            GetPointLocation(routePoint);
            if (isEmptyElement)
                return routePoint;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                await ReadRoutePointExtensionsAsync(routePoint).ConfigureAwait(false);
                                break;
                            default:
                                if (!await ProcessPointFieldAsync(routePoint).ConfigureAwait(false))
                                    await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxTrack> ReadGpxTrackAsync()
        {
            GpxTrack track = new GpxTrack();
            if (reader.IsEmptyElement)
                return track;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                track.Name = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case GpxSymbol.Comment:
                                track.Comment = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "desc":
                                track.Description = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "src":
                                track.Source = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "link":
                                track.Links.Add(await ReadGpxLinkAsync().ConfigureAwait(false));
                                break;
                            case "number":
                                track.Number = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "type":
                                track.Type = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case GpxSymbol.TrackSegment:
                                track.Segments.Add(await ReadGpxTrackSegmentAsync().ConfigureAwait(false));
                                break;
                            case "extensions":
                                await ReadTrackExtensionsAsync(track).ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxTrackSegment> ReadGpxTrackSegmentAsync()
        {
            GpxTrackSegment segment = new GpxTrackSegment();
            if (reader.IsEmptyElement)
                return segment;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.TrackPoint:
                                segment.Add(await ReadGpxTrackPointAsync().ConfigureAwait(false));
                                break;
                            case "extensions":
                                await ReadTrackSegmentExtensionsAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxTrackPoint> ReadGpxTrackPointAsync()
        {
            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GpxTrackPoint trackPoint = new GpxTrackPoint();
            GetPointLocation(trackPoint);
            if (isEmptyElement) return trackPoint;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "extensions":
                                await ReadTrackPointExtensionsAsync(trackPoint).ConfigureAwait(false);
                                break;
                            default:
                                if (!await ProcessPointFieldAsync(trackPoint).ConfigureAwait(false))
                                    await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxPerson> ReadGpxPersonAsync()
        {
            GpxPerson person = new GpxPerson();
            if (reader.IsEmptyElement)
                return person;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case GpxSymbol.Name:
                                person.Name = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "email":
                                person.Email = await ReadGpxEmailAsync().ConfigureAwait(false);
                                break;
                            case "link":
                                person.Link = await ReadGpxLinkAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxEmail> ReadGpxEmailAsync()
        {
            GpxEmail email = new GpxEmail();
            if (reader.IsEmptyElement)
                return email;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "id":
                                email.Id = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "domain":
                                email.Domain = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxLink> ReadGpxLinkAsync()
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

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "text":
                                link.Text = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "type":
                                link.MimeType = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxCopyright> ReadGpxCopyrightAsync()
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

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (reader.Name)
                        {
                            case "year":
                                copyright.Year = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "license":
                                copyright.Licence = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadWayPointExtensionsAsync(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
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
                                    await ReadGarminWayPointExtensionsAsync(wayPoint).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        if (reader.NamespaceURI == GpxNamespaces.DLG_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "level":
                                    wayPoint.Level = await ReadContentAsIntAsync().ConfigureAwait(false);
                                    break;
                                case "aliases":
                                    await ReadWayPointAliasesAsync(wayPoint).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private async Task ReadRouteExtensionsAsync(GpxRoute route)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "RouteExtension":
                                    await ReadGarminTrackOrRouteExtensionsAsync(route).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private async Task ReadRoutePointExtensionsAsync(GpxRoutePoint routePoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "RoutePointExtension":
                                    await ReadGarminRoutePointExtensionsAsync(routePoint).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private async Task ReadTrackExtensionsAsync(GpxTrack track)
        {
            if (reader.IsEmptyElement) return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (reader.LocalName)
                            {
                                case "TrackExtension":
                                    await ReadGarminTrackOrRouteExtensionsAsync(track).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private Task ReadTrackSegmentExtensionsAsync()
        {
            return SkipElementAsync();
        }

        private async Task ReadTrackPointExtensionsAsync(GpxTrackPoint trackPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
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
                                    await ReadGarminTrackPointExtensionsAsync(trackPoint).ConfigureAwait(false);
                                    break;
                                default:
                                    await SkipElementAsync().ConfigureAwait(false);
                                    break;
                            }

                            break;
                        }

                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName) throw new FormatException(reader.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private async Task ReadGarminWayPointExtensionsAsync(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case GpxSymbol.Proximity:
                                wayPoint.Proximity = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "Temperature":
                                wayPoint.Temperature = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "Depth":
                                wayPoint.Depth = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "DisplayMode":
                                wayPoint.DisplayMode = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "Categories":
                                await ReadGarminCategoriesAsync(wayPoint).ConfigureAwait(false);
                                break;
                            case "Address":
                                wayPoint.Address = await ReadGarminGpxAddressAsync().ConfigureAwait(false);
                                break;
                            case "PhoneNumber":
                                wayPoint.Phones.Add(await ReadGarminGpxPhoneAsync().ConfigureAwait(false));
                                break;
                            case "Samples":
                                wayPoint.Samples = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "Expiration":
                                wayPoint.Expiration = await ReadContentAsDateTimeAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadGarminTrackOrRouteExtensionsAsync(GpxTrackOrRoute trackOrRoute)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "DisplayColor":
                                trackOrRoute.DisplayColor = (GpxColor)Enum.Parse(typeof(GpxColor),
                                    await ReadContentAsStringAsync().ConfigureAwait(false), false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadGarminRoutePointExtensionsAsync(GpxRoutePoint routePoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "rpt":
                                routePoint.RoutePoints.Add(await ReadGarminAutoRoutePointAsync().ConfigureAwait(false));
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadGarminTrackPointExtensionsAsync(GpxTrackPoint trackPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Temperature":
                            case "atemp":
                                trackPoint.Temperature = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "wtemp":
                                trackPoint.WaterTemperature = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "Depth":
                            case "depth":
                                trackPoint.Depth = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "hr":
                                trackPoint.HeartRate = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "cad":
                                trackPoint.Cadence = await ReadContentAsIntAsync().ConfigureAwait(false);
                                break;
                            case "speed":
                                trackPoint.Speed = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "course":
                                trackPoint.Course = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            case "bearing":
                                trackPoint.Bearing = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadGarminCategoriesAsync(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "Category":
                                wayPoint.Categories.Add(await ReadContentAsStringAsync().ConfigureAwait(false));
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task ReadWayPointAliasesAsync(GpxWayPoint wayPoint)
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "alias":
                                wayPoint.Aliases.Add(await ReadContentAsStringAsync().ConfigureAwait(false));
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxPoint> ReadGarminAutoRoutePointAsync()
        {
            GpxPoint point = new GpxPoint();

            string elementName = reader.Name;
            bool isEmptyElement = reader.IsEmptyElement;

            GetPointLocation(point);
            if (isEmptyElement)
                return point;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        await SkipElementAsync().ConfigureAwait(false);
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != elementName)
                            throw new FormatException(reader.Name);
                        return point;
                }
            }

            throw new FormatException(elementName);
        }

        private async Task<GpxAddress> ReadGarminGpxAddressAsync()
        {
            GpxAddress address = new GpxAddress();
            if (reader.IsEmptyElement)
                return address;

            string elementName = reader.Name;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "StreetAddress":

                                if (string.IsNullOrEmpty(address.StreetAddress))
                                {
                                    address.StreetAddress = await ReadContentAsStringAsync().ConfigureAwait(false);
                                    break;
                                }

                                address.StreetAddress += " " + await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;

                            case "City":
                                address.City = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "State":
                                address.State = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "Country":
                                address.Country = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            case "PostalCode":
                                address.PostalCode = await ReadContentAsStringAsync().ConfigureAwait(false);
                                break;
                            default:
                                await SkipElementAsync().ConfigureAwait(false);
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

        private async Task<GpxPhone> ReadGarminGpxPhoneAsync()
        {
            return new GpxPhone
            {
                Category = reader.GetAttribute("Category", GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE),
                Number = await ReadContentAsStringAsync().ConfigureAwait(false)
            };
        }

        private async Task SkipElementAsync()
        {
            if (reader.IsEmptyElement)
                return;

            string elementName = reader.Name;
            int depth = reader.Depth;

            while (await reader.ReadAsync().ConfigureAwait(false))
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
                    case GpxSymbol.Latitude:
                        point.Latitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                    case GpxSymbol.Longitude:
                        point.Longitude = Angle.FromDegrees(double.Parse(reader.Value, CultureInfo.InvariantCulture.NumberFormat));
                        break;
                }
            }
        }

        private async Task<bool> ProcessPointFieldAsync(GpxPoint point)
        {
            switch (reader.Name)
            {
                case GpxSymbol.Elevation:
                    point.Elevation = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case GpxSymbol.Time:
                    point.Time = await ReadContentAsDateTimeAsync().ConfigureAwait(false);
                    return true;
                case "magvar":
                    point.MagneticVar = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case "geoidheight":
                    point.GeoidHeight = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case GpxSymbol.Name:
                    point.Name = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case GpxSymbol.Comment:
                    point.Comment = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "desc":
                    point.Description = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "src":
                    point.Source = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "link":
                    point.Links.Add(await ReadGpxLinkAsync().ConfigureAwait(false));
                    return true;
                case "sym":
                    point.Symbol = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "type":
                    point.Type = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "fix":
                    point.FixType = await ReadContentAsStringAsync().ConfigureAwait(false);
                    return true;
                case "sat":
                    point.Satelites = await ReadContentAsIntAsync().ConfigureAwait(false);
                    return true;
                case "hdop":
                    point.Hdop = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case "vdop":
                    point.Vdop = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case "pdop":
                    point.Pdop = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case "ageofdgpsdata":
                    point.AgeOfData = await ReadContentAsDoubleAsync().ConfigureAwait(false);
                    return true;
                case "dgpsid":
                    point.DgpsId = await ReadContentAsIntAsync().ConfigureAwait(false);
                    return true;
            }

            return false;
        }

        private async Task<string> ReadContentAsStringAsync()
        {
            if (reader.IsEmptyElement)
                throw new FormatException(reader.Name);

            string elementName = reader.Name;
            string result = string.Empty;

            while (await reader.ReadAsync().ConfigureAwait(false))
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

        private async Task<int> ReadContentAsIntAsync()
        {
            string value = await ReadContentAsStringAsync().ConfigureAwait(false);
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private async Task<double> ReadContentAsDoubleAsync()
        {
            string value = await ReadContentAsStringAsync().ConfigureAwait(false);
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        private async Task<DateTime> ReadContentAsDateTimeAsync()
        {
            string value = await ReadContentAsStringAsync().ConfigureAwait(false);
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}