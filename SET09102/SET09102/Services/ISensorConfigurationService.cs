using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SET09102.Models;

namespace SET09102.Services
{
    public interface ISensorConfigurationService
    {
        Task<IEnumerable<Sensor>> GetAllSensorsAsync();
        Task<SensorConfiguration> GetSensorConfigurationAsync(string sensorId);
        Task UpdateSensorConfigurationAsync(string sensorId, SensorConfiguration configuration);
        Task<bool> CheckFirmwareUpdateAsync(string sensorId);
        Task UpdateFirmwareAsync(string sensorId, Action<double> progressCallback);
    }
} 