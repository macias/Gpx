using System;
using System.Collections.Generic;

namespace Gpx
{
    public static partial class GeoCalculator
    {
        private struct Vector
        {
            public static readonly Vector Zero = new Vector();

            public double X { get; }
            public double Y { get; }
            public double Z { get; }

            public Vector(double x, double y, double z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            internal double Length()
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }

            public override bool Equals(object obj)
            {
                if (obj is Vector vec)
                    return Equals(vec);
                else
                    return false;
            }
            public bool Equals(Vector obj)
            {
                return this.X == obj.X && this.Y == obj.Y && this.Z == obj.Z;
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
            }

            public static Vector operator /(Vector v, double scalar)
            {
                return new Vector(v.X / scalar, v.Y / scalar, v.Z / scalar);
            }

            public static bool operator ==(Vector a, Vector b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Vector a, Vector b)
            {
                return !(a == b);
            }

            public Vector CrossProduct(Vector b)
            {
                double x = this.Y * b.Z - this.Z * b.Y;
                double y = this.X * b.Z - this.Z * b.X;
                double z = this.X * b.Y - this.Y * b.X;

                return new Vector(x, y, z);
            }

            public double DotProduct(Vector b)
            {
                return this.X * b.X + this.Y * b.Y + this.Z * b.Z;
            }

            public IGeoPoint ToGeoPoint()
            {
                Vector unit_vec = this / this.Length();

                Angle lat = Angle.FromRadians(Math.Asin(unit_vec.Z));
                Angle lon = Angle.FromRadians(Math.Atan2(unit_vec.X, unit_vec.Y));

                return new GeoPoint() { Latitude = lat, Longitude = lon };
            }

        }
    }
}