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
            for (double cm=0;cm<=100;cm+=10)
            {
                Length len = Length.FromCentimeters(cm);
                double res = len.Centimeters;
                Assert.Equal(cm, res);
            }
        }

    }
}
