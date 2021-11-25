using Gpx.Implementation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gpx
{
    public interface IStreamProgress
    {
        long Position { get; }
        long Length { get; }
    }    
}