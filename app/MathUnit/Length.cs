using System;
using System.Globalization;

namespace MathUnit
{
    public readonly struct Length : IComparable<Length>, IEquatable<Length>
    {
        public static readonly Length MinValue = new Length(double.MinValue);
        public static readonly Length MaxValue = new Length(double.MaxValue);
        public static readonly Length PositiveInfinity = new Length(double.PositiveInfinity);
        public static readonly Length Zero = new Length(0);

        private readonly double meters;

        public bool IsZero => this.meters == 0;
        public bool IsPositiveInfinity => double.IsPositiveInfinity(this.meters);
        public double Meters => this.meters;
        public double Centimeters => this.meters * 100;
        public double Millimeters => this.meters * 1000.0;
        public double Kilometers => this.meters / 1000.0;

        private Length(double meters)
        {
            this.meters = meters;
        }

        public static Length FromMeters(double meters)
        {
            return new Length(meters);
        }
        public static Length FromKilometers(double kilometers)
        {
            return new Length(kilometers * 1000.0);
        }
        public static Length FromMillimeters(double millimeters)
        {
            return new Length(millimeters / 1000.0);
        }
        public static Length FromCentimeters(double centimeters)
        {
            return new Length(centimeters / 100.0);
        }

        public static Length operator *(Length length, double scalar)
        {
            return new Length(length.meters * scalar);
        }
        public static Length operator *(double scalar, Length length)
        {
            return new Length(length.meters * scalar);
        }
        public static Length operator /(Length length, double scalar)
        {
            return new Length(length.meters / scalar);
        }
        public static double operator /(Length a, Length b)
        {
            return a.meters / b.meters;
        }
        public static Speed operator /(Length len, TimeSpan time)
        {
            return Speed.FromMetersPerSecond(len.meters / time.TotalSeconds);
        }
        public static TimeSpan operator /(Length len, Speed speed)
        {
            return TimeSpan.FromSeconds(len.meters / speed.MetersPerSecond);
        }
        public static Length operator +(Length len1, Length len2)
        {
            return new Length(len1.meters + len2.meters);
        }
        public static Length operator -(Length len1, Length len2)
        {
            return new Length(len1.meters - len2.meters);
        }
        public static Length operator -(Length len)
        {
            return new Length(-len.meters);
        }

        public static Length Max(Length len1, Length len2)
        {
            return new Length(Math.Max(len1.meters, len2.meters));
        }
        public static Length Min(Length len1, Length len2)
        {
            return new Length(Math.Min(len1.meters, len2.meters));
        }
        public Length Abs()
        {
            return new Length(Math.Abs(this.meters));
        }

        public static bool operator >(Length a, Length b)
        {
            return a.meters > b.meters;
        }
        public static bool operator >=(Length a, Length b)
        {
            return a.meters >= b.meters;
        }
        public static bool operator <(Length a, Length b)
        {
            return a.meters < b.meters;
        }
        public static bool operator <=(Length a, Length b)
        {
            return a.meters <= b.meters;
        }
        public static bool operator ==(Length a, Length b)
        {
            return a.meters == b.meters;
        }
        public static bool operator !=(Length a, Length b)
        {
            return a.meters != b.meters;
        }

        public override string ToString()
        {
            return $"{this.meters.ToString(CultureInfo.InvariantCulture)}m";
        }
        public string ToString(string format)
        {
            return $"{meters.ToString(format)}m";
        }

        public override bool Equals(object obj)
        {
            if (obj is Length length)
                return Equals(length);
            else
                return false;
        }
        public bool Equals(Length obj)
        {
            return this.meters == obj.meters;
        }
        public override int GetHashCode()
        {
            return this.meters.GetHashCode();
        }

        public Length Min(Length other)
        {
            return this < other ? this : other;
        }
        public Length Max(Length other)
        {
            return this > other ? this : other;
        }

        public int CompareTo(Length other)
        {
            return this.meters.CompareTo(other.meters);
        }

        public int Sign()
        {
            return Math.Sign(this.meters);
        }
    }
}
