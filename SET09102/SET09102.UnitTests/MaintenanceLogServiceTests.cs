using Microsoft.Data.Sqlite;
using SET09102.Models;
using SET09102.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xunit;
using Moq;
using SET09102.OperationsManager.Pages;

namespace SET09102.Tests
{
    public class MaintenanceLogServiceTests
    {
        private readonly Mock<DatabaseService> _mockDatabaseService;
        private readonly MaintenanceLogService _maintenanceLogService;
        private readonly SqliteConnection _mockConnection;

        public MaintenanceLogServiceTests()
        {
            // Setup in-memory SQLite database for testing
            _mockConnection = new SqliteConnection("Data Source=:memory:");
            _mockConnection.Open();
            
            var command = _mockConnection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS maintenance_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    sensor_id INTEGER NOT NULL,
                    maintenance_type TEXT NOT NULL,
                    performed_by TEXT NOT NULL,
                    notes TEXT,
                    created_at TEXT NOT NULL
                );";
            command.ExecuteNonQuery();
            
            _mockDatabaseService = new Mock<DatabaseService>();
            _mockDatabaseService.Setup(x => x.GetConnection()).Returns(_mockConnection);
            
            _maintenanceLogService = new MaintenanceLogService(_mockDatabaseService.Object);
        }
        
        [Fact]
        public async Task CreateMaintenanceLogAsync_ValidLog_ReturnsId()
        {
            var testLog = new MaintenanceLog
            {
                SensorId = 1,
                MaintenanceType = "Calibration",
                PerformedBy = "Test Engineer",
                Notes = "Routine calibration",
                CreatedAt = DateTime.Now
            };
            
            int logId = await _maintenanceLogService.CreateMaintenanceLogAsync(testLog);
            
            Assert.True(logId > 0);
            _mockDatabaseService.Verify(x => x.GetConnection(), Times.Once);
        }
        
        [Fact]
        public async Task CreateMaintenanceLogAsync_NullLog_ThrowsArgumentNullException()
        {
            MaintenanceLog nullLog = null;
            
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _maintenanceLogService.CreateMaintenanceLogAsync(nullLog));
        }
        
        [Fact]
        public async Task GetMaintenanceLogsAsync_ValidSensorId_ReturnsLogs()
        {
            int sensorId = 1;
            
            var insertCommand = _mockConnection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO maintenance_logs (
                    sensor_id, maintenance_type, performed_by, notes, created_at
                ) VALUES (
                    @SensorId, @MaintenanceType, @PerformedBy, @Notes, @CreatedAt
                );";
                
            insertCommand.Parameters.AddWithValue("@SensorId", sensorId);
            insertCommand.Parameters.AddWithValue("@MaintenanceType", "Test Type");
            insertCommand.Parameters.AddWithValue("@PerformedBy", "Test User");
            insertCommand.Parameters.AddWithValue("@Notes", "Test Notes");
            insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCommand.ExecuteNonQuery();
            
            var logs = await _maintenanceLogService.GetMaintenanceLogsAsync(sensorId);
            
            Assert.NotNull(logs);
            Assert.IsType<ObservableCollection<MaintenanceLog>>(logs);
            Assert.Single(logs);
            Assert.Equal(sensorId, logs[0].SensorId);
        }
        
        [Fact]
        public async Task GetMaintenanceLogsAsync_NonExistentSensorId_ReturnsEmptyCollection()
        {
            int nonExistentSensorId = 999;
            
            var logs = await _maintenanceLogService.GetMaintenanceLogsAsync(nonExistentSensorId);
            
            Assert.NotNull(logs);
            Assert.Empty(logs);
        }
        
        [Fact]
        public async Task GetRecentMaintenanceLogsAsync_LimitSet_ReturnsLimitedLogs()
        {
            int limit = 3;
            
            for (int i = 0; i < 5; i++)
            {
                var insertCommand = _mockConnection.CreateCommand();
                insertCommand.CommandText = @"
                    INSERT INTO maintenance_logs (
                        sensor_id, maintenance_type, performed_by, notes, created_at
                    ) VALUES (
                        @SensorId, @MaintenanceType, @PerformedBy, @Notes, @CreatedAt
                    );";
                    
                insertCommand.Parameters.AddWithValue("@SensorId", i + 1);
                insertCommand.Parameters.AddWithValue("@MaintenanceType", $"Type {i}");
                insertCommand.Parameters.AddWithValue("@PerformedBy", $"User {i}");
                insertCommand.Parameters.AddWithValue("@Notes", $"Notes {i}");
                insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now.AddHours(-i).ToString("yyyy-MM-dd HH:mm:ss"));
                insertCommand.ExecuteNonQuery();
            }
            
            var logs = await _maintenanceLogService.GetRecentMaintenanceLogsAsync(limit);
            
            Assert.NotNull(logs);
            Assert.Equal(limit, logs.Count);
        }
        
        public void Dispose()
        {
            _mockConnection?.Dispose();
        }
    }
}