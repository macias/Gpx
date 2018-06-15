namespace Gpx
{
    public interface IGeoPoint
    {
        double Latitude { get; } // degrees
        double Longitude { get; }

        string ToString(string format);
    }
}