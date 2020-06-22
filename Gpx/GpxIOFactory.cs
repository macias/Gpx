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
        public static IDisposable CreateReader(MemoryStream stream, out IGpxReader reader)
        {
            var result = new GpxReader(stream);
            reader = result;
            return result;
        }
        public static IDisposable CreateReader(string filepath, out IGpxReader reader)
        {
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(filepath));
            IDisposable result = CreateReader(stream, out  reader);
            return new Disposable(result,stream);
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
            return new Disposable(result,stream);
        }
    }
}