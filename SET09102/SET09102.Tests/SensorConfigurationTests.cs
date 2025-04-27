using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using Xunit;

namespace SET09102.Tests
{
    public class SensorConfigurationTests
    {
        private readonly MockSensorConfigurationService _service;

        public SensorConfigurationTests()
        {
            _service = new MockSensorConfigurationService();
        }

        [Fact]
        public async Task GetAllSensorsAsync_ReturnsAllSensors()
        {
            // Act
            var sensors = await _service.GetAllSensorsAsync();

            // Assert
            Assert.NotNull(sensors);
            Assert.Equal(3, sensors.Count());
        }

        [Fact]
        public async Task GetSensorConfigurationAsync_ReturnsCorrectConfiguration()
        {
            // Arrange
            var sensorId = "1";

            // Act
            var config = await _service.GetSensorConfigurationAsync(sensorId);

            // Assert
            Assert.NotNull(config);
            Assert.Equal(60, config.PollingInterval);
            Assert.Equal(50.0, config.AlertThreshold);
            Assert.Equal("1.0.0", config.FirmwareVersion);
        }

        [Fact]
        public async Task UpdateSensorConfigurationAsync_UpdatesConfiguration()
        {
            // Arrange
            var sensorId = "1";
            var newConfig = new SensorConfiguration
            {
                PollingInterval = 120,
                AlertThreshold = 75.0,
                FirmwareVersion = "1.1.0"
            };

            // Act
            await _service.UpdateSensorConfigurationAsync(sensorId, newConfig);
            var updatedConfig = await _service.GetSensorConfigurationAsync(sensorId);

            // Assert
            Assert.Equal(newConfig.FirmwareVersion, updatedConfig.FirmwareVersion);
        }

        [Fact]
        public async Task CheckFirmwareUpdateAsync_ReturnsCorrectUpdateStatus()
        {
            // Arrange
            var sensorId = "1"; // Version 1.0.0

            // Act
            var updateAvailable = await _service.CheckFirmwareUpdateAsync(sensorId);

            // Assert
            Assert.True(updateAvailable);
        }

        [Fact]
        public async Task UpdateFirmwareAsync_UpdatesFirmwareVersion()
        {
            // Arrange
            var sensorId = "1";
            double progress = 0;

            // Act
            await _service.UpdateFirmwareAsync(sensorId, p => progress = p);
            var config = await _service.GetSensorConfigurationAsync(sensorId);

            // Assert
            Assert.Equal(1.0, progress);
            Assert.Equal("2.0.0", config.FirmwareVersion);
        }
    }
} 