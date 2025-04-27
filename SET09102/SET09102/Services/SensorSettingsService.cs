using Microsoft.Data.Sqlite;
using SET09102.Models;
using System.Text.Json;

namespace SET09102.Services
{
    /// <summary>
    /// Service for managing sensor settings and global application settings.
    /// </summary>
    /// <remarks>
    /// This service provides functionality to store, retrieve, and update configuration settings
    /// for individual sensors as well as application-wide settings.
    /// </remarks>
    public class SensorSettingsService
    {
        private readonly string _dbPath;
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorSettingsService"/> class.
        /// </summary>
        /// <param name="databaseService">The database service to use for data operations.</param>
        public SensorSettingsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _dbPath = databaseService.GetDatabasePath();
        }

        /// <summary>
        /// Initializes the database tables required for sensor settings.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error initializing the database tables.</exception>
        /// <remarks>
        /// Creates the 'sensor_settings' and 'global_settings' tables if they don't already exist.
        /// </remarks>
        public async Task InitializeAsync()
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS sensor_settings (
                        id INTEGER PRIMARY KEY,
                        sensor_id INTEGER NOT NULL,
                        settings_json TEXT NOT NULL,
                        last_updated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (sensor_id) REFERENCES sensors(id)
                    );
                    
                    CREATE TABLE IF NOT EXISTS global_settings (
                        id INTEGER PRIMARY KEY,
                        key TEXT NOT NULL UNIQUE,
                        value TEXT NOT NULL,
                        last_updated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );";
                
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing sensor settings database", ex);
            }
        }

        /// <summary>
        /// Retrieves settings for a specific sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve settings for.</param>
        /// <returns>
        /// A <see cref="SensorSettings"/> object containing the sensor's settings, or default settings if none exist.
        /// </returns>
        /// <remarks>
        /// If there are no settings for the specified sensor, a new <see cref="SensorSettings"/> object with default values is returned.
        /// </remarks>
        public async Task<SensorSettings> GetSensorSettingsAsync(int sensorId)
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT settings_json FROM sensor_settings WHERE sensor_id = @SensorId";
                command.Parameters.AddWithValue("@SensorId", sensorId);
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    string json = result.ToString();
                    return JsonSerializer.Deserialize<SensorSettings>(json) ?? new SensorSettings();
                }
                
                return new SensorSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving sensor settings: {ex.Message}");
                return new SensorSettings(); 
            }
        }

        /// <summary>
        /// Saves settings for a specific sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to save settings for.</param>
        /// <param name="settings">The settings to save.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the operation was successful.
        /// </returns>
        /// <remarks>
        /// If settings already exist for the specified sensor, they will be updated. Otherwise, a new record will be created.
        /// </remarks>
        public async Task<bool> SaveSensorSettingsAsync(int sensorId, SensorSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings);
                
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO sensor_settings (sensor_id, settings_json, last_updated)
                    VALUES (@SensorId, @SettingsJson, CURRENT_TIMESTAMP)
                    ON CONFLICT(sensor_id) DO UPDATE SET
                        settings_json = @SettingsJson,
                        last_updated = CURRENT_TIMESTAMP";
                
                command.Parameters.AddWithValue("@SensorId", sensorId);
                command.Parameters.AddWithValue("@SettingsJson", json);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving sensor settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves a global application setting by key.
        /// </summary>
        /// <param name="key">The key of the setting to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the setting does not exist.</param>
        /// <returns>
        /// The value of the setting, or <paramref name="defaultValue"/> if it does not exist.
        /// </returns>
        /// <remarks>
        /// If the setting does not exist, it will be created with the default value.
        /// </remarks>
        public async Task<string> GetGlobalSettingAsync(string key, string defaultValue = "")
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT value FROM global_settings WHERE key = @Key";
                command.Parameters.AddWithValue("@Key", key);
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    return result.ToString();
                }
                
                await SetGlobalSettingAsync(key, defaultValue);
                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving global setting: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a global application setting.
        /// </summary>
        /// <param name="key">The key of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the operation was successful.
        /// </returns>
        /// <remarks>
        /// If the setting already exists, it will be updated. Otherwise, a new record will be created.
        /// </remarks>
        public async Task<bool> SetGlobalSettingAsync(string key, string value)
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO global_settings (key, value, last_updated)
                    VALUES (@Key, @Value, CURRENT_TIMESTAMP)
                    ON CONFLICT(key) DO UPDATE SET
                        value = @Value,
                        last_updated = CURRENT_TIMESTAMP";
                
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Value", value);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving global setting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the calibration schedule for multiple sensors.
        /// </summary>
        /// <param name="sensorIds">The IDs of the sensors to update.</param>
        /// <param name="calibrationIntervalDays">The number of days between calibrations.</param>
        /// <returns>
        /// A <see cref="bool"/> indicating whether the operation was successful.
        /// </returns>
        /// <remarks>
        /// This method updates both the sensor settings and the next calibration date in the sensors table.
        /// The operation is performed as a transaction to ensure data consistency.
        /// </remarks>
        public async Task<bool> UpdateCalibrationScheduleAsync(IEnumerable<int> sensorIds, int calibrationIntervalDays)
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // For each sensor, get current settings, update calibration interval, and save
                    foreach (int sensorId in sensorIds)
                    {
                        var settings = await GetSensorSettingsAsync(sensorId);
                        settings.CalibrationIntervalDays = calibrationIntervalDays;
                        
                        var command = connection.CreateCommand();
                        command.CommandText = @"
                            UPDATE sensors
                            SET next_calibration = date(last_calibration, '+' || @IntervalDays || ' days')
                            WHERE id = @SensorId";
                        
                        command.Parameters.AddWithValue("@IntervalDays", calibrationIntervalDays);
                        command.Parameters.AddWithValue("@SensorId", sensorId);
                        
                        await command.ExecuteNonQueryAsync();
                        
                        await SaveSensorSettingsAsync(sensorId, settings);
                    }
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error in transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating calibration schedule: {ex.Message}");
                return false;
            }
        }
    }
}