using System;
using System.Threading.Tasks;
using System.Xml;

namespace Gpx
{
    public interface IGpxTrackPointReader<TTrackPoint>
        where TTrackPoint : GpxTrackPoint, new()
    {
        bool TryReadBody(XmlReader xmlReader, TTrackPoint point);
        bool TryReadExtension(XmlReader xmlReader, TTrackPoint point);
    }
}