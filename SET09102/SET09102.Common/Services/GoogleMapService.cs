using SET09102.Common.Contracts;

namespace SET09102.Services;

public class GoogleMapService : IMapService
{
    public string GetDefaultUrl() => "https://maps.google.com/maps";

    public string GetMapUrl(double latitude, double longitude) => $"https://maps.google.com/maps?q={latitude},{longitude}";
}
