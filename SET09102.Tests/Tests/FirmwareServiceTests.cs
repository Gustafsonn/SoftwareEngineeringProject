using SET09102.Common.Services;
using System;
using Xunit;

namespace SET09102.Tests.Tests
{
    public class FirmwareServiceTests
    {
        private readonly FirmwareService _firmwareService;

        public FirmwareServiceTests()
        {
            _firmwareService = new FirmwareService();
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0")]
        [InlineData("2.1.0", "3.0.0")]
        [InlineData("3.2.1", "4.0.0")]
        [InlineData("9.9.9", "10.0.0")]
        [InlineData("0.0.0", "1.0.0")]
        public void GetNextVersion_ValidVersions_IncrementsVersionCorrectly(string currentVersion, string expectedNextVersion)
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
        [InlineData("version1.2.3")]
        public void GetNextVersion_InvalidVersions_ThrowsException(string invalidVersion)
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _firmwareService.GetNextVersion(invalidVersion));
            Assert.Contains("The firmware version was not in the expected format", ex.Message);
        }

        [Fact]
        public void GetNextVersion_VersionWithZeroMajor_IncrementsToOne()
        {
            // Arrange
            string currentVersion = "0.5.2";
            
            // Act
            var nextVersion = _firmwareService.GetNextVersion(currentVersion);
            
            // Assert
            Assert.Equal("1.0.0", nextVersion);
        }

        [Fact]
        public void GetNextVersion_VersionWithLargeMajor_IncrementsCorrectly()
        {
            // Arrange
            string currentVersion = "999.8.7";
            
            // Act
            var nextVersion = _firmwareService.GetNextVersion(currentVersion);
            
            // Assert
            Assert.Equal("1000.0.0", nextVersion);
        }
    }
}