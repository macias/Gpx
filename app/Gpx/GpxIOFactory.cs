using Gpx.Implementation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gpx
{
    public sealed class GpxIOFactory
    {
        private sealed class Disposable : IDisposable
        {
            private readonly IDisposable[] disposables;

            public Disposable(params IDisposable[] disposables)
            {
                this.disposables = disposables;
            }

            public void Dispose()
            {
                foreach (IDisposable disp in disposables)
                    disp.Dispose();
            }
        }
        public static IDisposable CreateAsyncReader(Stream stream, out IGpxAsyncReader reader)
        {
            var result = new GpxAsyncReader(stream);
            reader = result;
            return result;
        }
        public static IDisposable CreateReader(MemoryStream stream, out IGpxReader reader, out IStreamProgress streamProgress)
        {
            return CreateReader<GpxTrackPoint>(stream, trackPointReader: null, reader: out reader, streamProgress: out streamProgress);
        }
        public static IDisposable CreateReader<TTrackPoint>(MemoryStream stream, IGpxTrackPointReader<TTrackPoint> trackPointReader,
            out IGpxReader reader, out IStreamProgress streamProgress)
            where TTrackPoint : GpxTrackPoint, new()
        {
            var result = new GpxReader<TTrackPoint>(stream, trackPointReader ?? new NopTrackPointReader<TTrackPoint>());
            streamProgress = new StreamProgress(stream);
            reader = result;
            return result;
        }
        public static IDisposable CreateReader<TTrackPoint>(string filepath, IGpxTrackPointReader<TTrackPoint> trackPointReader,
            out IGpxReader reader, out IStreamProgress streamProgress)
            where TTrackPoint : GpxTrackPoint, new()
        {
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(filepath));
            IDisposable result = CreateReader(stream, trackPointReader, out reader, out streamProgress);
            return new Disposable(result, stream);
        }
        public static IDisposable CreateReader(string filepath, out IGpxReader reader, out IStreamProgress streamProgress)
        {
            return CreateReader<GpxTrackPoint>(filepath, trackPointReader: null, reader: out reader, streamProgress: out streamProgress);
        }

        public static IDisposable CreateWriter(Stream stream, out IGpxWriter writer)
        {
            var result = new GpxWriter(stream);
            writer = result;
            return result;
        }
        public static IDisposable CreateWriter(string path, out IGpxWriter writer)
        {
            var stream = new FileStream(path, FileMode.CreateNew);
            var result = CreateWriter(stream, out writer);
            return new Disposable(result, stream);
        }
    }
}