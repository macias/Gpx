using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MathUnit
{
    public readonly struct Angle : IEquatable<Angle>, IComparable<Angle>
    {
        public static Angle Zero { get; } = new Angle(0);
        public static Angle PI { get; } = Angle.FromRadians(Math.PI);
        public static Angle FullCircle { get; } = Angle.FromRadians(2 * Math.PI);

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

        private readonly double radians;

        public double Degrees => this.radians * 180 / Math.PI;

        public Angle Min(Angle other)
        {
            return this < other ? this : other;
        }
        public Angle Max(Angle other)
        {
            return this > other ? this : other;
        }

        public double Radians => this.radians;

        private Angle(double radians)
        {
            this.radians = radians;
        }

        public int Sign()
        {
            return Math.Sign(this.radians);
        }

        public double Sin()
        {
            return Math.Sin(this.radians);
        }
        public double Cos()
        {
            return Math.Cos(this.radians);
        }

        public Angle Normalize()
        {
            return new Angle(Mather.Mod(this.radians, 2 * Math.PI));
        }

        /*public static Angle Distance(Angle a, Angle b)
        {
            Angle diff = (a - b).Normalize();
            if (diff <= PI)
                return diff;
            else
                return 2 * PI - diff;
        }*/
        public static Angle operator -(Angle a, Angle b)
        {
            return new Angle(a.radians - b.radians);
        }
        public static Angle operator -(Angle a)
        {
            return new Angle(-a.radians);
        }

        public Angle Abs()
        {
            return this >= Angle.Zero ? this : -this;
        }
        public static Angle operator +(Angle a, Angle b)
        {
            return new Angle(a.radians + b.radians);
        }
        public static bool operator <(Angle a, Angle b)
        {
            return a.Radians < b.Radians;
        }
        public static bool operator <=(Angle a, Angle b)
        {
            return a.Radians <= b.Radians;
        }
        public static bool operator >(Angle a, Angle b)
        {
            return a.Radians > b.Radians;
        }
        public static bool operator >=(Angle a, Angle b)
        {
            return a.Radians >= b.Radians;
        }

        public static double operator /(Angle a, Angle b)
        {
            return a.radians / b.radians;
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
            return ToString(CultureInfo.InvariantCulture);
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

        public int CompareTo(Angle other)
        {
            return this.Radians.CompareTo(other.Radians);
        }

        public static bool operator ==(Angle a, Angle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Angle a, Angle b)
        {
            return !(a == b);
        }
    }
}
