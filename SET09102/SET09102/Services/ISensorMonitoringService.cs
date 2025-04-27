using System.Collections.Generic;
using System.Threading.Tasks;
using SET09102.Models;

namespace SET09102.Services
{
    public interface ISensorMonitoringService
    {
        Task<IEnumerable<SensorStatusInfo>> GetSensorStatusesAsync();
        Task<SensorStatusInfo> GetSensorStatusAsync(int sensorId);
        Task<bool> UpdateSensorStatusAsync(int sensorId, string status);
        Task<IEnumerable<SensorReading>> GetSensorReadingsAsync(int sensorId, DateTime startTime, DateTime endTime);
    }
} 