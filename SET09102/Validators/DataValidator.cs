using System;

namespace SET09102.Validators
{
    public interface IDataValidator
    {
        bool ValidateTemperature(double temperature);
        bool ValidateTimestamp(DateTime timestamp);
        bool ValidateData(object data);
    }

    public class DataValidator : IDataValidator
    {
        private readonly ILogger _logger;
        
        public DataValidator(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public bool ValidateTemperature(double temperature)
        {
            // Temperature range check (-50°C to 50°C as a reasonable range)
            if (temperature < -50 || temperature > 50)
            {
                _logger.LogError($"Invalid temperature value: {temperature}°C");
                return false;
            }
            
            return true;
        }
        
        public bool ValidateTimestamp(DateTime timestamp)
        {
            // Check if timestamp is in the future
            if (timestamp > DateTime.Now)
            {
                _logger.LogError($"Invalid timestamp: {timestamp} is in the future");
                return false;
            }
            
            return true;
        }
        
        public bool ValidateData(object data)
        {
            if (data == null)
            {
                _logger.LogError("Data is null");
                return false;
            }
            
            return true;
        }
    }
}