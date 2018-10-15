using System;
using System.Collections.Generic;

namespace Gpx
{
    public static partial class GeoCalculator
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

        public static string ToString<P>(this P point, string format)
            where P : IGeoPoint
        {
            return point.Latitude.ToString(format) + "," + point.Longitude.ToString(format);
        }

        /// <summary>
        /// gives nulls if there is no solution
        /// </summary>
        /// <param name="p2">if not null, p1 is also not null</param>
        public static void GetArcSegmentIntersection<P1, P2, P3, P4>(P1 startA, P2 endA, P3 startB, P4 endB,
            out IGeoPoint p1, out IGeoPoint p2, double accuracy = 1e-12)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
            where P3 : IGeoPoint
            where P4 : IGeoPoint
        {
            // https://www.movable-type.co.uk/scripts/latlong.html
            // http://blog.mbedded.ninja/mathematics/geometry/spherical-geometry/finding-the-intersection-of-two-arcs-that-lie-on-a-sphere

            if (!GetArcIntersection(startA, endA, startB, endB, out IGeoPoint cx1, out IGeoPoint cx2))
            {
                p1 = p2 = null;
                return;
            }

            Angle angle_a = angleBetween(startA, endA);
            Angle angle_b = angleBetween(startB, endB);
            // intersection has to lie within both segments
            Func<IGeoPoint, bool> within_segment = cx =>
            {
                Angle aa1 = angleBetween(cx, startA);
                Angle aa2 = angleBetween(cx, endA);
                Angle ab1 = angleBetween(cx, startB);
                Angle ab2 = angleBetween(cx, endB);
                return Math.Abs((aa1 + aa2 - angle_a).Radians) < accuracy && Math.Abs((ab1 + ab2 - angle_b).Radians) < accuracy;
            };

            p1 = within_segment(cx1) ? cx1 : null;
            p2 = within_segment(cx2) ? cx2 : null;

            if (p1 == null && p2 != null)
            {
                p1 = p2;
                p2 = null;
            }
        }

        private static Angle angleBetween<P1, P2>(P1 a, P2 b)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
        {
            // derived from cartesian dot product with unit radius, 
            // and using trig identities to reduce the number of trig functions used
            double lon_cos = (a.Longitude - b.Longitude).Cos();
            double lat_sub_cos = (a.Latitude - b.Latitude).Cos();
            double lat_add_cos = (a.Latitude + b.Latitude).Cos();

            double acos = Math.Acos((lon_cos * (lat_sub_cos + lat_add_cos) + (lat_sub_cos - lat_add_cos)) / 2.0);

            return Angle.FromRadians(acos);
        }

        public static bool GetArcIntersection<P1, P2, P3, P4>(P1 startA, P2 endA, P3 startB, P4 endB, out IGeoPoint p1, out IGeoPoint p2)
    where P1 : IGeoPoint
    where P2 : IGeoPoint
    where P3 : IGeoPoint
    where P4 : IGeoPoint
        {
            // http://blog.mbedded.ninja/mathematics/geometry/spherical-geometry/finding-the-intersection-of-two-arcs-that-lie-on-a-sphere

            Vector s = crossProduct(startA, endA);
            Vector t = crossProduct(startB, endB);

            Vector d = s.CrossProduct(t);

            if (d == Vector.Zero)
            {
                p1 = p2 = null;
                return false;
            }

            p1 = d.ToGeoPoint();
            p2 = oppositePoint(p1);

            return true;
        }

        private static IGeoPoint oppositePoint<P>(P p)
            where P : IGeoPoint
        {
            return new GeoPoint() { Latitude = -p.Latitude, Longitude = p.Longitude + Angle.FromRadians(Math.PI) };
        }

        private static Vector crossProduct<P1, P2>(P1 a, P2 b)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
        {
            // http://www.edwilliams.org/intersect.htm

            double a_lat_cos = a.Latitude.Cos();
            double a_lat_sin = a.Latitude.Sin();

            double b_lat_cos = b.Latitude.Cos();
            double b_lat_sin = b.Latitude.Sin();

            double x = a_lat_cos * a.Longitude.Cos() * b_lat_sin
                - a_lat_sin * b_lat_cos * b.Longitude.Cos();
            double y = a_lat_cos * a.Longitude.Sin() * b_lat_sin
                - a_lat_sin * b_lat_cos * b.Longitude.Sin();
            double z = a_lat_cos * b_lat_cos * (a.Longitude - b.Longitude).Sin();

            return new Vector(x, y, z);
        }

        public static IGeoPoint GetMidPoint<P1, P2>(P1 pointA, P2 pointB)
    where P1 : IGeoPoint
    where P2 : IGeoPoint
        {
            // https://www.movable-type.co.uk/scripts/latlong.html

            var λ1 = pointA.Longitude.Radians;
            var φ1 = pointA.Latitude.Radians;
            var λ2 = pointB.Longitude.Radians;
            var φ2 = pointB.Latitude.Radians;

            var Bx = Math.Cos(φ2) * Math.Cos(λ2 - λ1);
            var By = Math.Cos(φ2) * Math.Sin(λ2 - λ1);
            var φ3 = Math.Atan2(Math.Sin(φ1) + Math.Sin(φ2),
                                Math.Sqrt((Math.Cos(φ1) + Bx) * (Math.Cos(φ1) + Bx) + By * By));
            var λ3 = λ1 + Math.Atan2(By, Math.Cos(φ1) + Bx);

            return new GeoPoint() { Latitude = Angle.FromRadians(φ3), Longitude = Angle.FromRadians(λ3) };
        }

        public static Length GetDistance<P1, P2>(this P1 @this, P2 other)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
        {
            // https://en.wikipedia.org/wiki/Great-circle_distance#Computational_formulas

            double phi1 = @this.Latitude.Radians;
            double phi2 = other.Latitude.Radians;
            double lambda1 = @this.Longitude.Radians;
            double lambda2 = other.Longitude.Radians;

            double delta_phi = Math.Abs(phi1 - phi2);
            double delta_lambda = Math.Abs(lambda1 - lambda2);

            double cos_phi2 = Math.Cos(phi2);
            double sin_phi2 = Math.Sin(phi2);
            double sin_phi1 = Math.Sin(phi1);
            double cos_phi1 = Math.Cos(phi1);
            double cos_delta_lambda = Math.Cos(delta_lambda);

            double delta_sigma = Math.Atan2(
                Math.Sqrt(
                    Math.Pow(cos_phi2 * Math.Sin(delta_lambda), 2)
                +
                Math.Pow(cos_phi1 * sin_phi2 - sin_phi1 * cos_phi2 * cos_delta_lambda, 2)
                )

                ,

                sin_phi1 * sin_phi2 + cos_phi1 * cos_phi2 * cos_delta_lambda);

            return EarthRadius * Math.Abs(delta_sigma);
        }
        public static IGeoPoint GetDestinationPoint<P>(P point, Angle bearing, Length distance)
            where P : IGeoPoint
        {
            var φ1 = point.Latitude;
            var λ1 = point.Longitude;
            // https://www.movable-type.co.uk/scripts/latlong.html
            double d_cos = Math.Cos(distance / EarthRadius);
            double d_sin = Math.Sin(distance / EarthRadius);
            double φ1_sin = φ1.Sin();
            double φ1_cos = φ1.Cos();
            var φ2 = Angle.FromRadians(Math.Asin(φ1_sin * d_cos + φ1_cos * d_sin * bearing.Cos()));
            var λ2 = λ1 + Angle.FromRadians(Math.Atan2(bearing.Sin() * d_sin * φ1_cos, d_cos - φ1_sin * φ2.Sin()));

            return new GeoPoint() { Latitude = φ2, Longitude = λ2 };
        }

        public static Length GetDistanceToArcSegment<P1, P2, P3>(this P3 point, P1 segmentStart, P2 segmentEnd,
            out IGeoPoint crossPoint)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
            where P3 : IGeoPoint
        {
            return GetDistanceToArcSegment(point, segmentStart, segmentEnd, out crossPoint, computeCrossPoint: true);
        }
        private static Length GetDistanceToArcSegment<P1, P2, P3>(this P3 point, P1 segmentStart, P2 segmentEnd,
            out IGeoPoint crossPoint,bool computeCrossPoint)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
            where P3 : IGeoPoint
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            // CROSSARC Calculates the shortest distance in meters 
            // between an arc (defined by p1 and p2) and a third point, p3.

            double R = EarthRadius.Meters;

            // Prerequisites for the formulas
            double bear12 = GetBearing(segmentStart, segmentEnd).Radians;
            double bear13 = GetBearing(segmentStart, point).Radians;
            Length dis13 = segmentStart.GetDistance(point);

            // Is relative bearing obtuse?
            if (Math.Abs(bear13 - bear12) > (Math.PI / 2))
            {
                crossPoint = segmentStart;
                return dis13;
            }
            else
            {
                // Find the cross-track distance.
                double dxt = Math.Asin(Math.Sin(dis13.Meters / R) * Math.Sin(bear13 - bear12)) * R;
                // Is p4 beyond the arc?
                Length dis12 = segmentStart.GetDistance(segmentEnd);
                double dis14 = Math.Acos(Math.Cos(dis13.Meters / R) / Math.Cos(dxt / R)) * R;
                if (dis14 > dis12.Meters)
                {
                    crossPoint = segmentEnd;
                    return segmentEnd.GetDistance(point);
                }
                else
                {
                    crossPoint = computeCrossPoint
                        ? GetDestinationPoint(segmentStart, Angle.FromRadians(bear12), Length.FromMeters(dis14))
                        : null;

                    return Length.FromMeters(Math.Abs(dxt));
                }
            }
        }


        public static Length GetDistanceToArcSegment<P1, P2, P3>(this P3 point, P1 segmentStart, P2 segmentEnd)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
            where P3 : IGeoPoint
        {
            return GetDistanceToArcSegment(point, segmentStart, segmentEnd, out _,computeCrossPoint: false);
        }

        public static Length GetDistanceToArc<P1, P2, P3>(this P3 point, P1 arcA, P2 arcB)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
            where P3 : IGeoPoint
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            // simplification of the above

            // see also:
            // http://stackoverflow.com/a/20369652/6734314
            // http://www.movable-type.co.uk/scripts/latlong.html

            double R = EarthRadius.Meters;

            // Prerequisites for the formulas
            double bear12 = GetBearing(arcA, arcB).Radians;
            double bear13 = GetBearing(arcA, point).Radians;
            Length dis13 = arcA.GetDistance(point);

            // Find the cross-track distance.
            double dxt = Math.Asin(Math.Sin(dis13.Meters / R) * Math.Sin(bear13 - bear12)) * R;
            return Length.FromMeters(Math.Abs(dxt));
        }

        /// <summary>
        /// Finds the bearing from one lat/lon point to another.
        /// </summary>
        public static Angle GetBearing<P1, P2>(P1 start, P2 end)
            where P1 : IGeoPoint
            where P2 : IGeoPoint
        {
            // http://stackoverflow.com/questions/32771458/distance-from-lat-lng-point-to-minor-arc-segment
            double latA = start.Latitude.Radians;
            double lonA = start.Longitude.Radians;
            double latB = end.Latitude.Radians;
            double lonB = end.Longitude.Radians;

            double cos_lat_b = Math.Cos(latB);
            double y = Math.Sin(lonB - lonA) * cos_lat_b;
            double x = Math.Cos(latA) * Math.Sin(latB) - Math.Sin(latA) * cos_lat_b * Math.Cos(lonB - lonA);

            return Angle.FromRadians(Math.Atan2(y, x));
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