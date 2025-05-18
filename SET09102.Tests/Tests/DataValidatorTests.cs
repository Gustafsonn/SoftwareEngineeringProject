using System;
using Xunit;
using Moq;
using SET09102.Validators;
using SET09102.Services;

namespace SET09102.Tests
{
    public class DataValidatorTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly DataValidator _validator;

        public DataValidatorTests()
        {
            _mockLogger = new Mock<ILogger>();
            _validator = new DataValidator(_mockLogger.Object);
        }

        [Fact]
        public void ValidTemp_ReturnsTrue()
        {
            // Arrange
            double validTemperature = 25.0;

            // Act
            bool result = _validator.ValidateTemperature(validTemperature);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void OutOfRangeTemperature_ReturnsFalse()
        {
            // Arrange
            double invalidTemperature = -60.0;

            // Act
            bool result = _validator.ValidateTemperature(invalidTemperature);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void FutureTimestamp_ReturnsFalse()
        {
            // Arrange
            DateTime futureDate = DateTime.Now.AddDays(1);

            // Act
            bool result = _validator.ValidateTimestamp(futureDate);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void NullData_ReturnsFalse()
        {
            // Arrange
            object data = null;

            // Act
            bool result = _validator.ValidateData(data);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}