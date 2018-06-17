// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================

using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Gpx
{
    public enum GpxObjectType
    {
        None,
        Attributes,
        Metadata,
        WayPoint,
        Route,
        Track
    };

}