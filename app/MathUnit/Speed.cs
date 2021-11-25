using System;
using System.Globalization;

namespace MathUnit
{
    public readonly struct Speed : IComparable<Speed>, IEquatable<Speed>
    {
        public static readonly Speed MinValue = new Speed(double.MinValue);
        public static readonly Speed MaxValue = new Speed(double.MaxValue);
        public static readonly Speed PositiveInfinity = new Speed(double.PositiveInfinity);
        public static readonly Speed Zero = new Speed(0);

        private readonly double metersPerSecond;

        public bool IsZero => this.metersPerSecond == 0;
        public bool IsPositiveInfinity => double.IsPositiveInfinity(this.metersPerSecond);
        public double MetersPerSecond => this.metersPerSecond;
        public double KilometersPerSecond => this.metersPerSecond / 1_000;
        public double KilometersPerHour => this.metersPerSecond * 3_600 / 1_000;

        private Speed(double metersPerSecond)
        {
            this.metersPerSecond = metersPerSecond;
        }

        public static Speed FromMetersPerSecond(double metersPerSecond)
        {
            return new Speed(metersPerSecond);
        }
        public static Speed FromKilometersPerSecond(double kilometersPerSecond)
        {
            return new Speed(kilometersPerSecond * 1_000);
        }
        public static Speed FromKilometersPerHour(double kilometersPerHour)
        {
            return new Speed(kilometersPerHour * 1_000 / 3_600);
        }

        public static Speed operator *(Speed Speed, double scalar)
        {
            return new Speed(Speed.metersPerSecond * scalar);
        }
        public static Speed operator *(double scalar, Speed Speed)
        {
            return new Speed(Speed.metersPerSecond * scalar);
        }
        public static Speed operator /(Speed Speed, double scalar)
        {
            return new Speed(Speed.metersPerSecond / scalar);
        }
        public static double operator /(Speed a, Speed b)
        {
            return a.metersPerSecond / b.metersPerSecond;
        }
        public static Length operator *(Speed speed, TimeSpan time)
        {
            return Length.FromMeters(speed.metersPerSecond * time.TotalSeconds);
        }
        public static Speed operator +(Speed val1, Speed val2)
        {
            return new Speed(val1.metersPerSecond + val2.metersPerSecond);
        }
        public static Speed operator -(Speed val1, Speed val2)
        {
            return new Speed(val1.metersPerSecond - val2.metersPerSecond);
        }
        public static Speed operator -(Speed val)
        {
            return new Speed(-val.metersPerSecond);
        }

        public static Speed Max(Speed val1, Speed val2)
        {
            return new Speed(Math.Max(val1.metersPerSecond, val2.metersPerSecond));
        }
        public static Speed Min(Speed val1, Speed val2)
        {
            return new Speed(Math.Min(val1.metersPerSecond, val2.metersPerSecond));
        }
        public Speed Abs()
        {
            return new Speed(Math.Abs(this.metersPerSecond));
        }

        public static bool operator >(Speed a, Speed b)
        {
            return a.metersPerSecond > b.metersPerSecond;
        }
        public static bool operator >=(Speed a, Speed b)
        {
            return a.metersPerSecond >= b.metersPerSecond;
        }
        public static bool operator <(Speed a, Speed b)
        {
            return a.metersPerSecond < b.metersPerSecond;
        }
        public static bool operator <=(Speed a, Speed b)
        {
            return a.metersPerSecond <= b.metersPerSecond;
        }
        public static bool operator ==(Speed a, Speed b)
        {
            return a.metersPerSecond == b.metersPerSecond;
        }
        public static bool operator !=(Speed a, Speed b)
        {
            return a.metersPerSecond != b.metersPerSecond;
        }

        public override string ToString()
        {
            return $"{this.metersPerSecond.ToString(CultureInfo.InvariantCulture)}m/s";
        }
        public string ToString(string format)
        {
            return $"{metersPerSecond.ToString(format)}m/s";
        }

        public override bool Equals(object obj)
        {
            if (obj is Speed speed)
                return Equals(speed);
            else
                return false;
        }
        public bool Equals(Speed obj)
        {
            return this.metersPerSecond == obj.metersPerSecond;
        }
        public override int GetHashCode()
        {
            return this.metersPerSecond.GetHashCode();
        }

        public Speed Min(Speed other)
        {
            return this < other ? this : other;
        }
        public Speed Max(Speed other)
        {
            return this > other ? this : other;
        }

        public int CompareTo(Speed other)
        {
            return this.metersPerSecond.CompareTo(other.metersPerSecond);
        }

        public int Sign()
        {
            return Math.Sign(this.metersPerSecond);
        }
    }
}
