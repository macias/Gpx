using System;
using Xunit;

namespace MathUnit.Tests
{
    public class AngleTests
    {
        private const int precision = 10;

        [Fact]
        public void DistanceAroundZeroTest()
        {
            Angle a = Angle.FromDegrees(1);
            Angle b = Angle.FromDegrees(359);

            Angle a_b = Angle.Distance(a, b);
            Angle b_a = Angle.Distance(b, a);

            Assert.Equal(2, a_b.Degrees, precision);
            Assert.Equal(2, b_a.Degrees, precision);
        }

        [Fact]
        public void SubtractionAroundZeroTest()
        {
            Angle a = Angle.FromDegrees(1);
            Angle b = Angle.FromDegrees(359);

            Angle a_b = (a - b).Normalize();
            Angle b_a = (b - a).Normalize();

            Assert.Equal(2, a_b.Degrees, precision);
            Assert.Equal(358, b_a.Degrees, precision);
        }

        [Fact]
        public void AdditionAroundZeroTest()
        {
            Angle a = Angle.FromDegrees(2);
            Angle b = Angle.FromDegrees(359);

            Angle a_b = (a + b).Normalize();
            Angle b_a = (b + a).Normalize();

            Assert.Equal(1, a_b.Degrees, precision);
            Assert.Equal(1, b_a.Degrees, precision);
        }
    }
}
