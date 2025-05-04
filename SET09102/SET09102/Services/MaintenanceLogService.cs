using Microsoft.Data.Sqlite;
using SET09102.Models;
using SET09102.OperationsManager.Pages;
using System.Collections.ObjectModel;

namespace SET09102.Services
{
    /// <summary>
    /// Service for managing maintenance logs for sensors.
    /// </summary>
    /// <remarks>
    /// This service provides functionality to:
    /// <list type="bullet">
    ///   <item><description>Create new maintenance logs</description></item>
    ///   <item><description>Retrieve maintenance logs for a sensor</description></item>
    ///   <item><description>Update existing maintenance logs</description></item>
    /// </list>
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MaintenanceLogService"/> class.
    /// </remarks>
    /// <param name="databaseService">The database service to use for data operations.</param>
    public class MaintenanceLogService(DatabaseService databaseService)
    {
        private readonly DatabaseService _databaseService = databaseService;

        /// <summary>
        /// Creates a new maintenance log in the database.
        /// </summary>
        /// <param name="log">The maintenance log to create.</param>
        /// <returns>A task representing the asynchronous operation with the created log's ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when log is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the database operation fails.</exception>
        public async Task<int> CreateMaintenanceLogAsync(MaintenanceLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO maintenance_logs (
                        sensor_id, 
                        maintenance_type, 
                        performed_by, 
                        notes, 
                        created_at
                    ) VALUES (
                        @SensorId, 
                        @MaintenanceType, 
                        @PerformedBy, 
                        @Notes, 
                        @CreatedAt
                    );
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@SensorId", log.SensorId);
                command.Parameters.AddWithValue("@MaintenanceType", log.MaintenanceType);
                command.Parameters.AddWithValue("@PerformedBy", log.PerformedBy);
                command.Parameters.AddWithValue("@Notes", log.Notes);
                command.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

                var id = await command.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create maintenance log", ex);
            }
        }

        /// <summary>
        /// Retrieves all maintenance logs for a specific sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to retrieve logs for.</param>
        /// <returns>A collection of maintenance logs for the specified sensor.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database operation fails.</exception>
        public async Task<ObservableCollection<MaintenanceLog>> GetMaintenanceLogsAsync(int sensorId)
        {
            var logs = new ObservableCollection<MaintenanceLog>();

            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        id, 
                        sensor_id, 
                        maintenance_type, 
                        performed_by, 
                        notes, 
                        created_at 
                    FROM 
                        maintenance_logs 
                    WHERE 
                        sensor_id = @SensorId 
                    ORDER BY 
                        created_at DESC";

                command.Parameters.AddWithValue("@SensorId", sensorId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new MaintenanceLog
                    {
                        Id = reader.GetInt32(0),
                        SensorId = reader.GetInt32(1),
                        MaintenanceType = reader.GetString(2),
                        PerformedBy = reader.GetString(3),
                        Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        CreatedAt = reader.GetDateTime(5)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve maintenance logs for sensor {sensorId}", ex);
            }

            return logs;
        }

        /// <summary>
        /// Gets recent maintenance logs for all sensors.
        /// </summary>
        /// <param name="limit">The maximum number of logs to retrieve.</param>
        /// <returns>A collection of recent maintenance logs.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database operation fails.</exception>
        public async Task<ObservableCollection<MaintenanceLog>> GetRecentMaintenanceLogsAsync(int limit = 20)
        {
            var logs = new ObservableCollection<MaintenanceLog>();

            try
            {
                using var connection = _databaseService.GetConnection();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        id, 
                        sensor_id, 
                        maintenance_type, 
                        performed_by, 
                        notes, 
                        created_at 
                    FROM 
                        maintenance_logs 
                    ORDER BY 
                        created_at DESC 
                    LIMIT @Limit";

                command.Parameters.AddWithValue("@Limit", limit);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new MaintenanceLog
                    {
                        Id = reader.GetInt32(0),
                        SensorId = reader.GetInt32(1),
                        MaintenanceType = reader.GetString(2),
                        PerformedBy = reader.GetString(3),
                        Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        CreatedAt = reader.GetDateTime(5)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve recent maintenance logs", ex);
            }

            return logs;
        }
    }
}