using System;
using System.Collections.Generic;
using System.Linq;
using SET09102.Validators;
using SET09102.Models;

namespace SET09102.Services
{
    public interface IDataStorageService
    {
        bool SaveData(SensorData data);
        List<SensorData> GetData(int sensorId);
        bool IsSensorIdUnique(int sensorId);
    }

    public class DataStorageService : IDataStorageService
    {
        private readonly IDataValidator _validator;
        private readonly ILogger _logger;
        private readonly List<SensorData> _dataStore = new List<SensorData>();

        public DataStorageService(IDataValidator validator, ILogger logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool SaveData(SensorData data)
        {
            if (!_validator.ValidateData(data))
            {
                _logger.LogError("Invalid data provided");
                return false;
            }

            if (!_validator.ValidateTemperature(data.Temperature))
            {
                _logger.LogError($"Invalid temperature value: {data.Temperature}°C");
                return false;
            }

            if (!_validator.ValidateTimestamp(data.Timestamp))
            {
                _logger.LogError($"Invalid timestamp: {data.Timestamp}");
                return false;
            }

            if (!IsSensorIdUnique(data.SensorId))
            {
                _logger.LogError($"Duplicate sensor ID: {data.SensorId}");
                return false;
            }

            _dataStore.Add(data);
            _logger.LogInformation($"Data saved successfully for sensor ID: {data.SensorId}");
            return true;
        }

        public List<SensorData> GetData(int sensorId)
        {
            return _dataStore.FindAll(d => d.SensorId == sensorId);
        }

        public bool IsSensorIdUnique(int sensorId)
        {
            return !_dataStore.Exists(d => d.SensorId == sensorId);
        }
    }
}