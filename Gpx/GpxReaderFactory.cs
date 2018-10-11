// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

using System.IO;

namespace Gpx
{
    public sealed class GpxReaderFactory
    {
        public static IGpxReader Create(Stream stream)
        {
            return new Implementation.GpxReader(stream);
        }
    }
}