namespace Gpx
{
    public class GpxTrackPoint : GpxPoint
    {
        // GARMIN_EXTENSIONS, GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Temperature { get; set; }

        public double? Depth { get; set; }

        // GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? WaterTemperature { get; set; }

        public int? HeartRate { get; set; }

        public int? Cadence { get; set; }

        // GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Speed { get; set; }

        public double? Course { get; set; }

        public double? Bearing { get; set; }

        public bool HasGarminExtensions
        {
            get { return Temperature != null || Depth != null; }
        }

        public bool HasGarminTrackpointExtensionsV1
        {
            get { return WaterTemperature != null || HeartRate != null || Cadence != null; }
        }

        public bool HasGarminTrackpointExtensionsV2
        {
            get { return Speed != null || Course != null || Bearing != null; }
        }

        public bool HasExtensions
        {
            get { return HasGarminExtensions || HasGarminTrackpointExtensionsV1 || HasGarminTrackpointExtensionsV2; }
        }
    }
}