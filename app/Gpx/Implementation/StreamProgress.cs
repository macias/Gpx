using System.IO;

namespace Gpx.Implementation
{
    internal sealed class StreamProgress : IStreamProgress
    {
        private readonly Stream stream;

        public long Position => stream.Position;
        public long Length => stream.Length;

        public StreamProgress(MemoryStream stream)
        {
            this.stream = stream;
        }

    }
}