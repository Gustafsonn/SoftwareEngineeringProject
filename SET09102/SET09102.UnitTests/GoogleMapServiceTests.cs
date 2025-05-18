using SET09102.Services;

namespace SET09102.UnitTests;

public class GoogleMapServiceTests
{
    private readonly GoogleMapService _mapService;

    public GoogleMapServiceTests()
    {
        _mapService = new GoogleMapService();
    }

    [Fact]
    public void GetDefaultUrl_ReturnsExpectedUrl()
    {
        // Act
        var url = _mapService.GetDefaultUrl();

        // Assert
        Assert.Equal("https://maps.google.com/maps", url);
    }

    [Theory]
    [InlineData(37.7749, -122.4194, "https://maps.google.com/maps?q=37.7749,-122.4194")]
    [InlineData(0, 0, "https://maps.google.com/maps?q=0,0")]
    [InlineData(-90, 180, "https://maps.google.com/maps?q=-90,180")]
    public void GetMapUrl_ReturnsCorrectUrl(double lat, double lng, string expectedUrl)
    {
        // Act
        var url = _mapService.GetMapUrl(lat, lng);

        // Assert
        Assert.Equal(expectedUrl, url);
    }
}
