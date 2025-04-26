using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SET09102.Models;

namespace SET09102.Services
{
    public class MockSensorConfigurationService : ISensorConfigurationService
    {
        private readonly List<Sensor> _mockSensors = new List<Sensor>
        {
            new Sensor { Id = "1", Name = "Air Quality Sensor 1", Type = "Air", Location = "Building A", IsActive = true, FirmwareVersion = "1.0.0" },
            new Sensor { Id = "2", Name = "Water Quality Sensor 1", Type = "Water", Location = "River Station", IsActive = true, FirmwareVersion = "1.1.0" },
            new Sensor { Id = "3", Name = "Weather Station 1", Type = "Weather", Location = "Main Office", IsActive = true, FirmwareVersion = "2.0.0" }
        };

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
        {
            await Task.Delay(500); // Simulate network delay
            return _mockSensors;
        }

        public async Task<SensorConfiguration> GetSensorConfigurationAsync(string sensorId)
        {
            await Task.Delay(300); // Simulate network delay
            return new SensorConfiguration
            {
                PollingInterval = 60,
                AlertThreshold = 50.0,
                FirmwareVersion = _mockSensors.Find(s => s.Id == sensorId)?.FirmwareVersion ?? "1.0.0"
            };
        }

        public async Task UpdateSensorConfigurationAsync(string sensorId, SensorConfiguration configuration)
        {
            await Task.Delay(500); // Simulate network delay
            var sensor = _mockSensors.Find(s => s.Id == sensorId);
            if (sensor != null)
            {
                sensor.FirmwareVersion = configuration.FirmwareVersion;
            }
        }

        public async Task<bool> CheckFirmwareUpdateAsync(string sensorId)
        {
            await Task.Delay(300); // Simulate network delay
            var sensor = _mockSensors.Find(s => s.Id == sensorId);
            return sensor?.FirmwareVersion != "2.0.0"; // Simulate update available if not on latest version
        }

        public async Task UpdateFirmwareAsync(string sensorId, Action<double> progressCallback)
        {
            var sensor = _mockSensors.Find(s => s.Id == sensorId);
            if (sensor != null)
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    await Task.Delay(100); // Simulate update progress
                    progressCallback(i / 100.0);
                }
                sensor.FirmwareVersion = "2.0.0";
            }
        }
    }
} 