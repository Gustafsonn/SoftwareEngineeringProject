using Microsoft.Data.Sqlite;
using Moq;
using SET09102.Models;
using SET09102.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace SET09102.Tests
{
    public class SensorSettingsServiceTests
    {
        private readonly Mock<DatabaseService> _mockDatabaseService;
        private readonly SqliteConnection _mockConnection;
        private readonly SensorSettingsService _sensorSettingsService;

        public SensorSettingsServiceTests()
        {
            // Setup in-memory SQLite database for testing
            _mockConnection = new SqliteConnection("Data Source=:memory:");
            _mockConnection.Open();
            
            SetupDatabase();
            
            _mockDatabaseService = new Mock<DatabaseService>();
            _mockDatabaseService.Setup(x => x.GetConnection()).Returns(_mockConnection);
            _mockDatabaseService.Setup(x => x.GetDatabasePath()).Returns(":memory:");
            
            _sensorSettingsService = new SensorSettingsService(_mockDatabaseService.Object);
        }
        
        private void SetupDatabase()
        {
            var command = _mockConnection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS sensors (
                    id INTEGER PRIMARY KEY,
                    name TEXT NOT NULL,
                    location TEXT NOT NULL,
                    type TEXT NOT NULL,
                    status TEXT NOT NULL,
                    last_calibration DATETIME,
                    next_calibration DATETIME
                );
                
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
            
            command.ExecuteNonQuery();
            
            command = _mockConnection.CreateCommand();
            command.CommandText = @"
                INSERT INTO sensors (id, name, location, type, status, last_calibration, next_calibration)
                VALUES (1, 'Test Sensor', 'Test Location', 'Temperature', 'Active', '2023-01-01', '2023-02-01');";
            
            command.ExecuteNonQuery();
        }

        [Fact]
        public async Task InitializeAsync_CreatesTablesSuccessfully()
        {
            await _sensorSettingsService.InitializeAsync();
            
            var command = _mockConnection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND (name='sensor_settings' OR name='global_settings');";
            
            using var reader = command.ExecuteReader();
            int tableCount = 0;
            while (reader.Read())
            {
                tableCount++;
            }
            
            Assert.Equal(2, tableCount);
        }
        
        [Fact]
        public async Task GetSensorSettingsAsync_NonExistentSensor_ReturnsDefaultSettings()
        {
            int nonExistentSensorId = 999;
            
            var settings = await _sensorSettingsService.GetSensorSettingsAsync(nonExistentSensorId);
            
            Assert.NotNull(settings);
            Assert.IsType<SensorSettings>(settings);
        }
        
        [Fact]
        public async Task SaveAndGetSensorSettingsAsync_ValidSensor_SavesAndRetrievesSettings()
        {
            int sensorId = 1;
            var testSettings = new SensorSettings
            {
                CalibrationIntervalDays = 30,
                AlertThreshold = 25.5,
            };
            
            bool saveResult = await _sensorSettingsService.SaveSensorSettingsAsync(sensorId, testSettings);
            var retrievedSettings = await _sensorSettingsService.GetSensorSettingsAsync(sensorId);
            
            Assert.True(saveResult);
            Assert.Equal(testSettings.CalibrationIntervalDays, retrievedSettings.CalibrationIntervalDays);
            Assert.Equal(testSettings.AlertThreshold, retrievedSettings.AlertThreshold);
        }
        
        [Fact]
        public async Task GetGlobalSettingAsync_NonExistentKey_CreatesWithDefaultValue()
        {
            string testKey = "TestKey";
            string defaultValue = "DefaultValue";
            
            string value = await _sensorSettingsService.GetGlobalSettingAsync(testKey, defaultValue);
            
            Assert.Equal(defaultValue, value);
            
            var command = _mockConnection.CreateCommand();
            command.CommandText = "SELECT value FROM global_settings WHERE key = @Key";
            command.Parameters.AddWithValue("@Key", testKey);
            
            var result = await command.ExecuteScalarAsync();
            Assert.Equal(defaultValue, result?.ToString());
        }
        
        [Fact]
        public async Task SetAndGetGlobalSettingAsync_ValidKey_SavesAndRetrievesSetting()
        {
            string testKey = "TestKey";
            string testValue = "TestValue";
            
            bool setResult = await _sensorSettingsService.SetGlobalSettingAsync(testKey, testValue);
            string retrievedValue = await _sensorSettingsService.GetGlobalSettingAsync(testKey);
            
            Assert.True(setResult);
            Assert.Equal(testValue, retrievedValue);
        }
        
        [Fact]
        public async Task UpdateCalibrationScheduleAsync_ValidSensorIds_UpdatesSettingsAndNextCalibration()
        {
            var sensorIds = new List<int> { 1 };
            int newCalibrationInterval = 60;
            
            var initialSettings = new SensorSettings
            {
                CalibrationIntervalDays = 30,
                AlertThreshold = 25.5,
            };
            await _sensorSettingsService.SaveSensorSettingsAsync(1, initialSettings);
            
            bool updateResult = await _sensorSettingsService.UpdateCalibrationScheduleAsync(sensorIds, newCalibrationInterval);
            var updatedSettings = await _sensorSettingsService.GetSensorSettingsAsync(1);
            
            Assert.True(updateResult);
            Assert.Equal(newCalibrationInterval, updatedSettings.CalibrationIntervalDays);
            
            var command = _mockConnection.CreateCommand();
            command.CommandText = "SELECT date(next_calibration) FROM sensors WHERE id = 1";
            var nextCalibration = await command.ExecuteScalarAsync();
            
            var command2 = _mockConnection.CreateCommand();
            command2.CommandText = "SELECT date(last_calibration, '+60 days') FROM sensors WHERE id = 1";
            var expectedNextCalibration = await command2.ExecuteScalarAsync();
            
            Assert.Equal(expectedNextCalibration?.ToString(), nextCalibration?.ToString());
        }
        
        [Fact]
        public async Task UpdateCalibrationScheduleAsync_InvalidSensorId_ReturnsFalse()
        {
            var invalidSensorIds = new List<int> { 999 }; 
            int newCalibrationInterval = 60;
            
            bool updateResult = await _sensorSettingsService.UpdateCalibrationScheduleAsync(invalidSensorIds, newCalibrationInterval);
            
            Assert.False(updateResult);
        }
        
        public void Dispose()
        {
            _mockConnection?.Dispose();
        }
    }
}