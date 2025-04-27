using Microsoft.Data.Sqlite;
using SET09102.Models;
using System.Text.Json;

namespace SET09102.Services
{
    public class SensorSettingsService
    {
        private readonly string _dbPath;
        private readonly DatabaseService _databaseService;

        public SensorSettingsService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _dbPath = databaseService.GetDatabasePath();
        }

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