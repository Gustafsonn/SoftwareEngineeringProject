using SET09102.Common.Services;

namespace SET09102.UnitTests;

public class FirmwareServiceTests
{
    private readonly FirmwareService _firmwareService;

    public FirmwareServiceTests()
    {
        _firmwareService = new FirmwareService();
    }

    [Theory]
    [InlineData("1.0.0", "2.0.0")]
    [InlineData("1.0.1", "2.0.0")]
    [InlineData("1.1.1", "2.0.0")]
    [InlineData("1.2.3", "2.0.0")]
    public void GetNextVersion_ValidVersion(string currentVersion, string expectedNextVersion)
    {
        // Act
        var nextVersion = _firmwareService.GetNextVersion(currentVersion);

        // Assert
        Assert.Equal(expectedNextVersion, nextVersion);
    }

    [Theory]
    [InlineData("abc.def.ghi")]
    [InlineData("1.2")]   
    [InlineData("")]
    [InlineData("1..3")]
    [InlineData("1.2.3.4")]
    public void GetNextVersion_InvalidVersion_ThrowsException(string invalidVersion)
    {
        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _firmwareService.GetNextVersion(invalidVersion));
        Assert.Contains("The firmware version was not in the expected format", ex.Message);
    }
}