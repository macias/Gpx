using System.IO;
using System.Xml;

namespace Gpx.Implementation
{
    internal sealed class NopTrackPointReader<TTrackPoint> : IGpxTrackPointReader<TTrackPoint>
        where TTrackPoint : GpxTrackPoint, new()
    {
        public bool TryReadBody(XmlReader xmlReader,TTrackPoint point)
        {
            return false;
        }

        public bool TryReadExtension(XmlReader xmlReader, TTrackPoint point)
        {
            return false;
        }
    }
}