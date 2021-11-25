using System;
using Xunit;

namespace MathUnit.Tests
{
    public class LengthTests
    {
        private const int precision = 10;


        [Fact]
        public void CentimetersTest()
        {
            for (double cm = 0; cm <= 100; cm += 10)
            {
                Length len = Length.FromCentimeters(cm);
                double res = len.Centimeters;
                Assert.Equal(cm, res);
            }
        }

        [Fact]
        public void TimeTest()
        {
            var speed = Speed.FromKilometersPerHour(10);
            for (double km = 0; km <= 100; km += 10)
            {
                Length len = Length.FromKilometers(km);
                var time = len / speed;
                Assert.Equal(km / 10, time.TotalHours);
            }
        }

    }
}
