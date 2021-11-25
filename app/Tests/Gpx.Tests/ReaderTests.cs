using System;
using System.Linq;
using System.Xml;
using Xunit;

namespace Gpx.Tests
{   
    public class ReaderTests
    {
        private static GpxTrack readTrack<TTrackPoint>(string path, IGpxTrackPointReader<TTrackPoint> trackPointReader = null)
            where TTrackPoint : GpxTrackPoint, new()
        {
            GpxTrack track = null;

            using (GpxIOFactory.CreateReader(path, trackPointReader, out IGpxReader reader, out _))
            {
                while (reader.Read(out GpxObjectType type))
                {
                    switch (type)
                    {
                        case GpxObjectType.Metadata:
                            break;
                        case GpxObjectType.WayPoint:
                            break;
                        case GpxObjectType.Route:
                            break;
                        case GpxObjectType.Track:
                            {
                                if (track == null)
                                    track = reader.Track;
                                else
                                    throw new InvalidOperationException("Track is already read");
                                break;
                            }
                    }
                }
            }

            if (track == null)
                throw new NullReferenceException("Track was not read");

            return track;

        }

        [Fact]
        public void ReadingForcedExtensionTest()
        {
            GpxTrack track = readTrack("Data/forced-extension.gpx", new ProximityTrackPointReader());

            Assert.Equal(DateTimeOffset.Parse("2002-01-25T15:11:00Z"), track.Segments.Single().TrackPoints[0].Time);
            Assert.Equal(6, (track.Segments.Single().TrackPoints[0] as ProximityTrackPoint).Proximity);
            Assert.Equal(DateTimeOffset.Parse("2002-01-25T15:11:01Z"), track.Segments.Single().TrackPoints[1].Time);
            Assert.Equal(12, (track.Segments.Single().TrackPoints[1] as ProximityTrackPoint).Proximity);
        }
    }
}
