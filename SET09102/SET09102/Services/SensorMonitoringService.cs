using Microsoft.Data.Sqlite;
using SET09102.Models;
using System.Collections.ObjectModel;

namespace SET09102.Services
{
    /// <summary>
    /// Service for monitoring and managing sensor operational status
    /// </summary>
    public class SensorMonitoringService
    {
        private readonly DatabaseService _databaseService;
        private readonly SensorService _sensorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorMonitoringService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service to use for data operations.</param>
        public SensorMonitoringService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _sensorService = new SensorService(_databaseService.GetDatabasePath());
        }

        /// <summary>
        /// Gets all sensors with their current status
        /// </summary>
        /// <returns>A collection of sensors with status information</returns>
        public async Task<ObservableCollection<Sensor>> GetSensorsWithStatusAsync()
        {
            return await _sensorService.GetSensorsAsync();
        }

        /// <summary>
        /// Updates the status of a sensor
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to update</param>
        /// <param name="newStatus">The new status to set</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public async Task<bool> UpdateSensorStatusAsync(int sensorId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                return false;

            // Validate status
            newStatus = newStatus.ToLower();
            if (newStatus != "operational" && newStatus != "maintenance" && newStatus != "offline")
                return false;

            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE sensors 
                    SET status = @Status, updated_at = @UpdatedAt
                    WHERE id = @SensorId";

                command.Parameters.AddWithValue("@Status", newStatus);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@SensorId", sensorId);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating sensor status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets sensors that need attention (maintenance or offline)
        /// </summary>
        /// <returns>A collection of sensors that need attention</returns>
        public async Task<ObservableCollection<Sensor>> GetSensorsNeedingAttentionAsync()
        {
            var allSensors = await _sensorService.GetSensorsAsync();
            var needAttention = allSensors.Where(s => s.IsActive && 
                (s.Status.ToLower() == "maintenance" || s.Status.ToLower() == "offline" || s.IsDueForCalibration));
            
            return new ObservableCollection<Sensor>(needAttention);
        }

        /// <summary>
        /// Gets the count of sensors by status
        /// </summary>
        /// <param name="activeOnly">Whether to count only active sensors</param>
        /// <returns>A dictionary with status counts</returns>
        public async Task<Dictionary<string, int>> GetSensorStatusCountsAsync(bool activeOnly = true)
        {
            var allSensors = await _sensorService.GetSensorsAsync();
            var filteredSensors = activeOnly ? allSensors.Where(s => s.IsActive) : allSensors;
            
            var counts = new Dictionary<string, int>
            {
                { "operational", filteredSensors.Count(s => s.Status.ToLower() == "operational") },
                { "maintenance", filteredSensors.Count(s => s.Status.ToLower() == "maintenance") },
                { "offline", filteredSensors.Count(s => s.Status.ToLower() == "offline") },
                { "total", filteredSensors.Count() }
            };
            
            return counts;
        }

        /// <summary>
        /// Updates the firmware version of a sensor
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to update</param>
        /// <param name="newVersion">The new firmware version</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public async Task<bool> UpdateSensorFirmwareAsync(int sensorId, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(newVersion))
                return false;

            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE sensors 
                    SET firmware_version = @FirmwareVersion, updated_at = @UpdatedAt
                    WHERE id = @SensorId";

                command.Parameters.AddWithValue("@FirmwareVersion", newVersion);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@SensorId", sensorId);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating sensor firmware: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Records a sensor calibration event
        /// </summary>
        /// <param name="sensorId">The ID of the sensor that was calibrated</param>
        /// <param name="intervalDays">The number of days until the next calibration</param>
        /// <returns>True if the record was created successfully, otherwise false</returns>
        public async Task<bool> RecordCalibrationAsync(int sensorId, int intervalDays = 90)
        {
            try
            {
                var now = DateTime.Now;
                var nextCalibration = now.AddDays(intervalDays);

                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE sensors 
                    SET last_calibration = @LastCalibration, 
                        next_calibration = @NextCalibration,
                        updated_at = @UpdatedAt
                    WHERE id = @SensorId";

                command.Parameters.AddWithValue("@LastCalibration", now);
                command.Parameters.AddWithValue("@NextCalibration", nextCalibration);
                command.Parameters.AddWithValue("@UpdatedAt", now);
                command.Parameters.AddWithValue("@SensorId", sensorId);

                int rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    // Also record the maintenance log
                    var logCommand = connection.CreateCommand();
                    logCommand.CommandText = @"
                        INSERT INTO sensor_maintenance_logs (
                            sensor_id, maintenance_type, performed_by, notes, created_at
                        ) VALUES (
                            @SensorId, 'Calibration', 'System', 'Routine calibration', @CreatedAt
                        )";
                    
                    logCommand.Parameters.AddWithValue("@SensorId", sensorId);
                    logCommand.Parameters.AddWithValue("@CreatedAt", now);
                    
                    await logCommand.ExecuteNonQueryAsync();
                }
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording calibration: {ex.Message}");
                return false;
            }
        }
    }
}