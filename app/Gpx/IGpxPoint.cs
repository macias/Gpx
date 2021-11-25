using System;
using System.Collections.Generic;
using MathUnit;

namespace Gpx
{
    public interface IGpxPoint
    {
        double? AgeOfData { get;  }
        string Comment { get;  }
        string Description { get;  }
        int? DgpsId { get;  }
        double? Elevation { get;  }
        GpxLink EmailLink { get; }
        string FixType { get;  }
        double? GeoidHeight { get;  }
        double? Hdop { get;  }
        GpxLink HttpLink { get; }
        Angle Latitude { get;  }
        IEnumerable<GpxLink> Links { get; }
        Angle Longitude { get;  }
        double? MagneticVar { get;  }
        string Name { get;  }
        double? Pdop { get;  }
        int? Satelites { get;  }
        string Source { get;  }
        string Symbol { get;  }
        DateTimeOffset? Time { get;  }
        string Type { get;  }
        double? Vdop { get;  }
    }
}