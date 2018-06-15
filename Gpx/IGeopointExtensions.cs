using System;
using System.Collections.Generic;

namespace Gpx
{
    public static class IGeopointExtensions
    {
        private static readonly Length EarthRadius = Length.FromKilometers(6371);
        private const double Radian = Math.PI / 180;

        private static double radiansToDegrees(double radians)
        {
            return radians / Radian;
        }
        private static double degreesToRadians(double degrees)
        {
            return degrees * Radian;
        }
        public static Length GetDistance(this IGeoPoint @this, IGeoPoint other)
        {
            // https://en.wikipedia.org/wiki/Great-circle_distance#Computational_formulas

            double phi1 = @this.Latitude;
            double phi2 = other.Latitude;
            double lambda1 = @this.Longitude;
            double lambda2 = other.Longitude;

            double delta_phi = Math.Abs(phi1 - phi2);
            double delta_lambda = Math.Abs(lambda1 - lambda2);

            phi1 = degreesToRadians(phi1);
            phi2 = degreesToRadians(phi2);
            lambda1 = degreesToRadians(lambda1);
            lambda2 = degreesToRadians(lambda2);
            delta_lambda = degreesToRadians(delta_lambda);
            delta_phi = degreesToRadians(delta_phi);

            double delta_sigma = Math.Atan2(
                Math.Sqrt(
                    Math.Pow(Math.Cos(phi2) * Math.Sin(delta_lambda), 2)
                +
                Math.Pow(Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(delta_lambda), 2)
                )

                ,

                (Math.Sin(phi1) * Math.Sin(phi2) + Math.Cos(phi1) * Math.Cos(phi2) * Math.Cos(delta_lambda)));


            return EarthRadius * delta_sigma;
        }

        public static Length GetDistanceToArcSegment(this IGeoPoint p3, IGeoPoint p1, IGeoPoint p2)
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            // CROSSARC Calculates the shortest distance in meters 
            // between an arc (defined by p1 and p2) and a third point, p3.

            double R = EarthRadius.Meters; 

            // Prerequisites for the formulas
            double bear12 = GetBearing(p1, p2);
            double bear13 = GetBearing(p1, p3);
            Length dis13 = p1.GetDistance(p3);

            // Is relative bearing obtuse?
            if (Math.Abs(bear13 - bear12) > (Math.PI / 2))
                return dis13;
            else
            {
                // Find the cross-track distance.
                double dxt = Math.Asin(Math.Sin(dis13.Meters / R) * Math.Sin(bear13 - bear12)) * R;
                // Is p4 beyond the arc?
                Length dis12 = p1.GetDistance(p2);
                double dis14 = Math.Acos(Math.Cos(dis13.Meters / R) / Math.Cos(dxt / R)) * R;
                if (dis14 > dis12.Meters)
                    return p2.GetDistance(p3);
                else
                    return Length.FromMeters(Math.Abs(dxt));
            }
        }
        public static Length GetDistanceToArc(this IGeoPoint p3, IGeoPoint p1, IGeoPoint p2)
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            // simplification of the above

            double R = EarthRadius.Meters;

            // Prerequisites for the formulas
            double bear12 = GetBearing(p1, p2);
            double bear13 = GetBearing(p1, p3);
            Length dis13 = p1.GetDistance(p3);

            // Find the cross-track distance.
            double dxt = Math.Asin(Math.Sin(dis13.Meters / R) * Math.Sin(bear13 - bear12)) * R;
            return Length.FromMeters(Math.Abs(dxt));
        }

        private static double GetBearing(IGeoPoint a,IGeoPoint b) // Finds the bearing from one lat/lon point to another.
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            double latA = degreesToRadians(a.Latitude);
            double lonA = degreesToRadians(a.Longitude);
            double latB = degreesToRadians(b.Latitude);
            double lonB = degreesToRadians(b.Longitude);

            return Math.Atan2(Math.Sin(lonB - lonA) * Math.Cos(latB),
                Math.Cos(latA) * Math.Sin(latB) - Math.Sin(latA) * Math.Cos(latB) * Math.Cos(lonB - lonA));
        }
        public static Length GetDistanceToArc_BUGGY(this IGeoPoint @this, IGeoPoint start, IGeoPoint end) 
        {
            // this method is buggy (a,s,e) gives other outcome than (a,e,s)

            // http://stackoverflow.com/a/20369652/6734314
            // see also:
            // http://www.movable-type.co.uk/scripts/latlong.html
            double lat1 = start.Latitude;
            double lon1 = start.Longitude;
            double lat2 = end.Latitude;
            double lon2 = end.Longitude;
            double lat3 = @this.Latitude;
            double lon3 = @this.Longitude;

            double y = Math.Sin(lon3 - lon1) * Math.Cos(lat3);
            double x = Math.Cos(lat1) * Math.Sin(lat3) - Math.Sin(lat1) * Math.Cos(lat3) * Math.Cos(lat3 - lat1);
            double bearing1 = radiansToDegrees(Math.Atan2(y, x));
            bearing1 = 360 - ((bearing1 + 360) % 360);

            double y2 = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
            double x2 = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lat2 - lat1);
            double bearing2 = radiansToDegrees(Math.Atan2(y2, x2));
            bearing2 = 360 - ((bearing2 + 360) % 360);

            double lat1Rads = degreesToRadians(lat1);
            double lat3Rads = degreesToRadians(lat3);
            double dLon = degreesToRadians(lon3 - lon1);

            double distanceAC = Math.Acos(Math.Sin(lat1Rads) * Math.Sin(lat3Rads)
                + Math.Cos(lat1Rads) * Math.Cos(lat3Rads) * Math.Cos(dLon)) * EarthRadius.Kilometers;
            double min_distance = Math.Abs(Math.Asin(Math.Sin(distanceAC / EarthRadius.Kilometers)
                * Math.Sin(degreesToRadians(bearing1) - degreesToRadians(bearing2))) * EarthRadius.Kilometers);

            return Length.FromKilometers(min_distance);
        }


        internal static IEnumerable<GpxPoint> ToGpxPoints<T>(this IEnumerable<T> points)
            where T : GpxPoint
        {
            var result = new List<GpxPoint>();

            foreach (T gpxPoint in points)
            {
                GpxPoint point = new GpxPoint
                {
                    Longitude = gpxPoint.Longitude,
                    Latitude = gpxPoint.Latitude,
                    Elevation = gpxPoint.Elevation,
                    Time = gpxPoint.Time
                };

                result.Add(point);
            }

            return result;
        }
    }
}