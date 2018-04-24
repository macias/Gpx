using System;

namespace Gpx
{
    public sealed class Length
    {
        public static readonly Length Zero = Length.FromMeters(0);

        private const double kmScale = 1000;

        private readonly double value;
        private readonly double scale;
        private readonly string unit;

        public bool IsZero { get { return this.value == 0; } }
        public double Meters { get { return this.value*scale; } }
        public double Kilometers { get { return this.value * scale/ kmScale; } }

        private Length(double meters,double scale,string unit)
        {
            this.value = meters;
            this.scale = scale;
            this.unit = unit;
        }

        public static Length FromMeters(double value)
        {
            return new Length(value,1,"m");
        }
        public static Length FromKilometers(double value)
        {
            return new Length(value, kmScale, "km");
        }

        public static Length operator *(Length length, double amount)
        {
            return new Length(length.value * amount, length.scale, length.unit);
        }
        public static Length operator +(Length len1, Length len2)
        {
            return new Length(len1.value + len2.value*len1.scale/len2.scale, len1.scale, len1.unit);
        }

        public override string ToString()
        {
            return ToString("0.00");
        }
        public string ToString(string format)
        {
            return value.ToString(format) + unit;
        }

        public override bool Equals(object obj)
        {
            if (this.GetType() == obj?.GetType())
                return Equals(obj as Length);
            else
                throw new ArgumentException();
        }
        public bool Equals(Length obj)
        {
            if (Object.ReferenceEquals(obj,null))
                return false;
            else if (Object.ReferenceEquals(obj, this))
                return true;

            return this.Meters == obj.Meters;
        }
        public override int GetHashCode()
        {
            return this.Meters.GetHashCode();
        }
    }
}
