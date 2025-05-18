using System;
using Xunit;
using Moq;
using SET09102.Validators;
using SET09102.Services;
using SET09102.Models;

namespace SET09102.Tests
{
    public class DataStorageServiceTests
    {
        private readonly Mock<IDataValidator> _mockValidator;
        private readonly Mock<ILogger> _mockLogger;
        private readonly DataStorageService _service;

        public DataStorageServiceTests()
        {
            _mockValidator = new Mock<IDataValidator>();
            _mockLogger = new Mock<ILogger>();
            _service = new DataStorageService(_mockValidator.Object, _mockLogger.Object);
            
            // Default behavior for validator
            _mockValidator.Setup(v => v.ValidateData(It.IsAny<object>())).Returns(true);
            _mockValidator.Setup(v => v.ValidateTemperature(It.IsAny<double>())).Returns(true);
            _mockValidator.Setup(v => v.ValidateTimestamp(It.IsAny<DateTime>())).Returns(true);
        }

        [Fact]
        public void ValidSave_ReturnsTrue()
        {
            // Arrange
            var validData = new SensorData 
            { 
                SensorId = 1, 
                Temperature = 25.0, 
                Timestamp = DateTime.Now 
            };

            // Act
            bool result = _service.SaveData(validData);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("1"))), Times.Once);
        }

        [Fact]
        public void DuplicateSensorID_ReturnsFalse()
        {
            // Arrange
            var data1 = new SensorData 
            { 
                SensorId = 1, 
                Temperature = 25.0, 
                Timestamp = DateTime.Now 
            };
            var data2 = new SensorData 
            { 
                SensorId = 1, 
                Temperature = 26.0, 
                Timestamp = DateTime.Now.AddHours(1) 
            };

            // Act
            _service.SaveData(data1);
            bool result = _service.SaveData(data2);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Duplicate"))), Times.Once);
        }

        [Fact]
        public void InvalidData_ReturnsFalse()
        {
            // Arrange
            var invalidData = new SensorData 
            { 
                SensorId = 1, 
                Temperature = -60.0, 
                Timestamp = DateTime.Now 
            };
            
            _mockValidator.Setup(v => v.ValidateTemperature(It.Is<double>(t => t == -60.0))).Returns(false);

            // Act
            bool result = _service.SaveData(invalidData);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("temperature"))), Times.Once);
        }

        [Fact]
        public void ErrorLogging_LogsCorrectly()
        {
            // Arrange
            var nullData = new SensorData { SensorId = 1, Temperature = 25.0, Timestamp = DateTime.Now };
            _mockValidator.Setup(v => v.ValidateData(It.IsAny<object>())).Returns(false);

            // Act
            bool result = _service.SaveData(nullData);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Invalid data"))), Times.Once);
        }
    }
}