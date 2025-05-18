namespace SET09102.Common.Contracts;

public interface IMapService
{
    string GetDefaultUrl();

    string GetMapUrl(double latitude, double longitude);
}
