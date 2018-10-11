using System;
using System.Globalization;

namespace Gpx
{
    public struct Angle
    {
        public static readonly Angle Zero = new Angle(0);

        public static Angle FromDegrees(double degrees)
        {
            return new Angle(degrees * Math.PI / 180.0);
        }
        public static Angle FromDegrees(int degrees, int minutes, double seconds)
        {
            return FromDegrees(degrees + minutes / 60.0 + seconds / 3600.0);
        }

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians);
        }

        private double radians;

        public double Degrees => this.radians * 180 / Math.PI;
        public double Radians => this.radians;

        private Angle(double radians)
        {
            this.radians = radians;
        }

        public double Sin()
        {
            return Math.Sin(this.radians);
        }
        public double Cos()
        {
            return Math.Cos(this.radians);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return new Angle(a.radians - b.radians);
        }
        public static Angle operator -(Angle a)
        {
            return new Angle(-a.radians);
        }
        public static Angle operator +(Angle a, Angle b)
        {
            return new Angle(a.radians + b.radians);
        }

        public static Angle operator *(Angle a, double scalar)
        {
            return new Angle(a.radians * scalar);
        }
        public static Angle operator /(Angle a, double scalar)
        {
            return new Angle(a.radians / scalar);
        }
        public static Angle operator *(double scalar, Angle a)
        {
            return new Angle(a.radians * scalar);
        }

        public override string ToString()
        {
            return $"{Degrees}°";
        }

        public string ToString(string format)
        {
            return $"{Degrees.ToString(format)}°";
        }

        public string ToString(CultureInfo culture)
        {
            return $"{Degrees.ToString(culture)}°";
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle angle)
                return Equals(angle);
            else
                return false;
        }

        public bool Equals(Angle obj)
        {
            return this.radians == obj.radians;
        }

        public override int GetHashCode()
        {
            return this.radians.GetHashCode();
        }

        public static bool operator ==(Angle a,Angle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Angle a, Angle b)
        {
            return !(a==b);
        }
    }
}
