// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

namespace Gpx
{
    public sealed class GpxTrackPoint : GpxPoint
    {
        // GARMIN_EXTENSIONS, GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Temperature
        {
            get { return Properties_.GetValueProperty<double>("Temperature"); }
            set { Properties_.SetValueProperty<double>("Temperature", value); }
        }

        public double? Depth
        {
            get { return Properties_.GetValueProperty<double>("Depth"); }
            set { Properties_.SetValueProperty<double>("Depth", value); }
        }

        // GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? WaterTemperature
        {
            get { return Properties_.GetValueProperty<double>("WaterTemperature"); }
            set { Properties_.SetValueProperty<double>("WaterTemperature", value); }
        }

        public int? HeartRate
        {
            get { return Properties_.GetValueProperty<int>("HeartRate"); }
            set { Properties_.SetValueProperty<int>("HeartRate", value); }
        }

        public int? Cadence
        {
            get { return Properties_.GetValueProperty<int>("Cadence"); }
            set { Properties_.SetValueProperty<int>("Cadence", value); }
        }

        // GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Speed
        {
            get { return Properties_.GetValueProperty<double>("Speed"); }
            set { Properties_.SetValueProperty<double>("Speed", value); }
        }

        public double? Course
        {
            get { return Properties_.GetValueProperty<double>("Course"); }
            set { Properties_.SetValueProperty<double>("Course", value); }
        }

        public double? Bearing
        {
            get { return Properties_.GetValueProperty<double>("Bearing"); }
            set { Properties_.SetValueProperty<double>("Bearing", value); }
        }

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