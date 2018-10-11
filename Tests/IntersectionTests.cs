using System;
using Gpx;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class IntersectionTests
    {
        // the precision of computation itself, it has nothing to do with geographic context
        private const double mathPrecision = 0.0000000001;

        [TestMethod]
        public void TestLinesNoIntersection()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18.01) };
            var start2 = start1;
            var end2 = end1;
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.IsNull(p1);
            Assert.IsNull(p2);
        }

        [TestMethod]
        public void TestPointsNoIntersection()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var end1 = start1;
            var start2 = start1;
            var end2 = end1;
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.IsNull(p1);
            Assert.IsNull(p2);
        }

        [TestMethod]
        public void TestSharedPointAtZero()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(0), Longitude = Angle.FromDegrees(0) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(0), Longitude = Angle.FromDegrees(0.01) };
            var start2 = start1;
            var end2 = new GeoPoint() { Latitude = Angle.FromDegrees(0.01), Longitude = Angle.FromDegrees(0) };
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.AreEqual(start1.Latitude.Degrees, p1.Latitude.Degrees, mathPrecision);
            Assert.AreEqual(start1.Longitude.Degrees, p1.Longitude.Degrees, mathPrecision);
            Assert.IsNull(p2);
        }

        // intersection of L-shape with one shared point, there are 4 variants, depending how do you define
        // start and end of each segment
        [TestMethod]
        public void TestSharedPoint1()
        {            
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18.01) };
            var start2 = start1;
            var end2 = new GeoPoint() { Latitude = Angle.FromDegrees(50.01), Longitude = Angle.FromDegrees(18) };
            GeoCalculator.GetArcSegmentIntersection(start1,end1,start2,end2,out IGeoPoint p1,out IGeoPoint p2);

            Assert.AreEqual(start1.Latitude.Degrees, p1.Latitude.Degrees, mathPrecision);
            Assert.AreEqual(start1.Longitude.Degrees, p1.Longitude.Degrees, mathPrecision);
            Assert.IsNull(p2);
        }

        [TestMethod]
        public void TestSharedPoint2()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18.01) };
            var start2 = new GeoPoint() { Latitude = Angle.FromDegrees(50.01), Longitude = Angle.FromDegrees(18) };
            var end2 = start1;
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.AreEqual(start1.Latitude.Degrees, p1.Latitude.Degrees, mathPrecision);
            Assert.AreEqual(start1.Longitude.Degrees, p1.Longitude.Degrees, mathPrecision);
            Assert.IsNull(p2);
        }

        [TestMethod]
        public void TestSharedPoint3()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18.01) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var start2 = end1;
            var end2 = new GeoPoint() { Latitude = Angle.FromDegrees(50.01), Longitude = Angle.FromDegrees(18) };
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.AreEqual(end1.Latitude.Degrees, p1.Latitude.Degrees, mathPrecision);
            Assert.AreEqual(end1.Longitude.Degrees, p1.Longitude.Degrees, mathPrecision);
            Assert.IsNull(p2);
        }

        [TestMethod]
        public void TestSharedPoint4()
        {
            var start1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18.01) };
            var end1 = new GeoPoint() { Latitude = Angle.FromDegrees(50), Longitude = Angle.FromDegrees(18) };
            var start2 = new GeoPoint() { Latitude = Angle.FromDegrees(50.01), Longitude = Angle.FromDegrees(18) };
            var end2 = end1;
            GeoCalculator.GetArcSegmentIntersection(start1, end1, start2, end2, out IGeoPoint p1, out IGeoPoint p2);

            Assert.AreEqual(end1.Latitude.Degrees, p1.Latitude.Degrees, mathPrecision);
            Assert.AreEqual(end1.Longitude.Degrees, p1.Longitude.Degrees, mathPrecision);
            Assert.IsNull(p2);
        }

    }
}