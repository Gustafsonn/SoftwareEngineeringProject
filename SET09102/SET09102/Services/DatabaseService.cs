using Microsoft.Data.Sqlite;
using SET09102.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SET09102.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private SqliteConnection? _connection;

        public DatabaseService()
        {
            try
            {
                string appDataPath = Path.Combine(FileSystem.AppDataDirectory, "EnvironmentalMonitoring");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                _dbPath = Path.Combine(appDataPath, "environmental_monitoring.db");
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database paths", ex);
            }
        }

        public string GetDatabasePath()
        {
            return _dbPath;
        }

        public SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
            {
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                _connection.Open();
            }
            return _connection;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    if (await VerifySchemaAsync())
                    {
                        await InsertSampleDataIfEmptyAsync();
                        await InsertSampleSensorsIfEmptyAsync();
                        await InsertSampleAlertsAndLogsIfEmptyAsync();
                        return;
                    }
                    else
                    {
                        File.Delete(_dbPath);
                    }
                }

                _connection = new SqliteConnection($"Data Source={_dbPath}");
                await _connection.OpenAsync();

                var createTablesSql = @"
                    CREATE TABLE IF NOT EXISTS air_quality (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        date TEXT NOT NULL,
                        time TEXT NOT NULL,
                        nitrogen_dioxide REAL,
                        sulphur_dioxide REAL,
                        pm25_particulate_matter REAL,
                        pm10_particulate_matter REAL
                    );

                    CREATE TABLE IF NOT EXISTS water_quality (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        date TEXT NOT NULL,
                        time TEXT NOT NULL,
                        nitrate REAL,
                        nitrite REAL,
                        phosphate REAL,
                        ec_cfu_per_100ml REAL
                    );

                    CREATE TABLE IF NOT EXISTS weather_conditions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        time TEXT NOT NULL,
                        temperature_2m REAL,
                        relative_humidity_2m REAL,
                        wind_speed_10m REAL,
                        wind_direction_10m REAL
                    );

                    CREATE TABLE IF NOT EXISTS sensors (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        site_id INTEGER NOT NULL,
                        name TEXT NOT NULL,
                        type TEXT NOT NULL,
                        unit TEXT NOT NULL,
                        location TEXT NOT NULL,
                        latitude REAL NOT NULL,
                        longitude REAL NOT NULL,
                        site_type TEXT NOT NULL,
                        zone TEXT NOT NULL,
                        agglomeration TEXT NOT NULL,
                        authority TEXT NOT NULL,
                        is_active BOOLEAN NOT NULL DEFAULT 1,
                        last_calibration DATETIME NOT NULL,
                        next_calibration DATETIME NOT NULL,
                        min_threshold REAL,
                        max_threshold REAL,
                        firmware_version TEXT,
                        last_maintenance DATETIME,
                        next_maintenance DATETIME,
                        status TEXT NOT NULL DEFAULT 'operational',
                        description TEXT,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS sensor_alerts (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        sensor_id INTEGER NOT NULL,
                        alert_type TEXT NOT NULL,
                        message TEXT NOT NULL,
                        severity TEXT NOT NULL,
                        threshold_value REAL,
                        measured_value REAL,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        resolved_at DATETIME,
                        status TEXT NOT NULL DEFAULT 'active',
                        FOREIGN KEY (sensor_id) REFERENCES sensors(id)
                    );

                    CREATE TABLE IF NOT EXISTS sensor_maintenance_logs (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        sensor_id INTEGER NOT NULL,
                        maintenance_type TEXT NOT NULL,
                        performed_by TEXT NOT NULL,
                        notes TEXT,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (sensor_id) REFERENCES sensors(id)
                    );
                    
                    CREATE TABLE IF NOT EXISTS environmental_data (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        data_type TEXT NOT NULL,
                        value REAL NOT NULL,
                        timestamp TEXT NOT NULL
                    );";

                using (var command = new SqliteCommand(createTablesSql, _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                var sensorService = new SensorService(_dbPath);
                await sensorService.InitializeSensorsAsync();

                await InsertSampleDataIfEmptyAsync();
                await InsertSampleSensorsIfEmptyAsync();
                await InsertSampleAlertsAndLogsIfEmptyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database", ex);
            }
        }

        private async Task InsertSampleDataIfEmptyAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM environmental_data";
            long count = (long)await countCommand.ExecuteScalarAsync();

            if (count == 0)
            {
                var sampleData = new List<EnvironmentalDataEntity>
                {
                    new EnvironmentalDataEntity { DataType = "Temperature", Value = 23.5, Timestamp = "2025-04-27 10:00:00" },
                    new EnvironmentalDataEntity { DataType = "Humidity", Value = 65.0, Timestamp = "2025-04-27 10:00:00" },
                    new EnvironmentalDataEntity { DataType = "AirQuality", Value = 42.0, Timestamp = "2025-04-27 10:00:00" }
                };

                foreach (var data in sampleData)
                {
                    await SaveEnvironmentalDataAsync(data);
                }
            }
        }

        private async Task InsertSampleSensorsIfEmptyAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM sensors";
            long count = (long)await countCommand.ExecuteScalarAsync();

            if (count == 0)
            {
                var sampleSensors = new List<Sensor>
                {
                    new Sensor
                    {
                        Id = 1,
                        SiteId = 1,
                        Name = "TempSensor01",
                        Type = "Temperature",
                        Unit = "Celsius",
                        Location = "North Field",
                        Latitude = 55.9533,
                        Longitude = -3.1883,
                        SiteType = "Rural",
                        Zone = "Zone A",
                        Agglomeration = "Edinburgh",
                        Authority = "City Council",
                        IsActive = true,
                        LastCalibration = DateTime.Now.AddMonths(-1),
                        NextCalibration = DateTime.Now.AddMonths(5),
                        FirmwareVersion = "1.2.3",
                        Status = "operational",
                        Description = "Temperature sensor in North Field",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Sensor
                    {
                        Id = 2,
                        SiteId = 2,
                        Name = "AirQualitySensor01",
                        Type = "AirQuality",
                        Unit = "PPM",
                        Location = "South Field",
                        Latitude = 55.9433,
                        Longitude = -3.1783,
                        SiteType = "Urban",
                        Zone = "Zone B",
                        Agglomeration = "Edinburgh",
                        Authority = "City Council",
                        IsActive = true,
                        LastCalibration = DateTime.Now.AddMonths(-2),
                        NextCalibration = DateTime.Now.AddMonths(4),
                        FirmwareVersion = "1.2.3",
                        Status = "maintenance",
                        Description = "Air quality sensor in South Field",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    new Sensor
                    {
                        Id = 3,
                        SiteId = 3,
                        Name = "HumiditySensor01",
                        Type = "Humidity",
                        Unit = "Percentage",
                        Location = "East Field",
                        Latitude = 55.9333,
                        Longitude = -3.1683,
                        SiteType = "Suburban",
                        Zone = "Zone C",
                        Agglomeration = "Edinburgh",
                        Authority = "City Council",
                        IsActive = false,
                        LastCalibration = DateTime.Now.AddMonths(-3),
                        NextCalibration = DateTime.Now.AddMonths(3),
                        FirmwareVersion = "1.2.3",
                        Status = "offline",
                        Description = "Humidity sensor in East Field",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };

                foreach (var sensor in sampleSensors)
                {
                    await SaveSensorAsync(sensor);
                }
            }
        }

        private async Task InsertSampleAlertsAndLogsIfEmptyAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            // Insert sample alerts
            var alertCountCommand = connection.CreateCommand();
            alertCountCommand.CommandText = "SELECT COUNT(*) FROM sensor_alerts";
            long alertCount = (long)await alertCountCommand.ExecuteScalarAsync();

            if (alertCount == 0)
            {
                var sampleAlerts = new List<SensorAlert>
                {
                    new SensorAlert
                    {
                        SensorId = 2, // AirQualitySensor01
                        AlertType = "ThresholdExceeded",
                        Message = "Air quality exceeds safe threshold",
                        Severity = "Critical",
                        ThresholdValue = 40.0,
                        MeasuredValue = 45.0,
                        CreatedAt = DateTime.Now,
                        Status = "active"
                    },
                    new SensorAlert
                    {
                        SensorId = 3, // HumiditySensor01
                        AlertType = "Offline",
                        Message = "Sensor is offline",
                        Severity = "High",
                        CreatedAt = DateTime.Now,
                        Status = "active"
                    }
                };

                foreach (var alert in sampleAlerts)
                {
                    await SaveSensorAlertAsync(alert);
                }
            }

            // Insert sample maintenance logs
            var logCountCommand = connection.CreateCommand();
            logCountCommand.CommandText = "SELECT COUNT(*) FROM sensor_maintenance_logs";
            long logCount = (long)await logCountCommand.ExecuteScalarAsync();

            if (logCount == 0)
            {
                var sampleLogs = new List<SensorMaintenanceLog>
                {
                    new SensorMaintenanceLog
                    {
                        SensorId = 2, // AirQualitySensor01
                        MaintenanceType = "Calibration",
                        PerformedBy = "Technician A",
                        Notes = "Recalibrated sensor due to drift",
                        CreatedAt = DateTime.Now.AddDays(-5)
                    },
                    new SensorMaintenanceLog
                    {
                        SensorId = 3, // HumiditySensor01
                        MaintenanceType = "Inspection",
                        PerformedBy = "Technician B",
                        Notes = "Found connectivity issue, scheduled repair",
                        CreatedAt = DateTime.Now.AddDays(-3)
                    }
                };

                foreach (var log in sampleLogs)
                {
                    await SaveSensorMaintenanceLogAsync(log);
                }
            }
        }

        private async Task<bool> VerifySchemaAsync()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    await connection.OpenAsync();
                    var tables = new[] { "air_quality", "water_quality", "weather_conditions", "sensors", "sensor_alerts", "sensor_maintenance_logs", "environmental_data" };
                    foreach (var table in tables)
                    {
                        using (var command = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'", connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            if (result == null)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Sensor>> GetSensorsAsync()
        {
            var sensors = new List<Sensor>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, site_id, name, type, unit, location, latitude, longitude, site_type, zone, agglomeration, authority, 
                       is_active, last_calibration, next_calibration, min_threshold, max_threshold, firmware_version, 
                       last_maintenance, next_maintenance, status, description, created_at, updated_at 
                FROM sensors";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sensors.Add(new Sensor
                {
                    Id = reader.GetInt32(0),
                    SiteId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Type = reader.GetString(3),
                    Unit = reader.GetString(4),
                    Location = reader.GetString(5),
                    Latitude = reader.GetDouble(6),
                    Longitude = reader.GetDouble(7),
                    SiteType = reader.GetString(8),
                    Zone = reader.GetString(9),
                    Agglomeration = reader.GetString(10),
                    Authority = reader.GetString(11),
                    IsActive = reader.GetBoolean(12),
                    LastCalibration = reader.GetDateTime(13),
                    NextCalibration = reader.GetDateTime(14),
                    MinThreshold = reader.IsDBNull(15) ? null : reader.GetDouble(15),
                    MaxThreshold = reader.IsDBNull(16) ? null : reader.GetDouble(16),
                    FirmwareVersion = reader.IsDBNull(17) ? null : reader.GetString(17),
                    LastMaintenance = reader.IsDBNull(18) ? null : reader.GetDateTime(18),
                    NextMaintenance = reader.IsDBNull(19) ? null : reader.GetDateTime(19),
                    Status = reader.GetString(20),
                    Description = reader.IsDBNull(21) ? null : reader.GetString(21),
                    CreatedAt = reader.GetDateTime(22),
                    UpdatedAt = reader.GetDateTime(23)
                });
            }

            return sensors;
        }

        public async Task<List<SensorAlert>> GetActiveSensorAlertsAsync(int sensorId)
        {
            var alerts = new List<SensorAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, sensor_id, alert_type, message, severity, threshold_value, measured_value, created_at, resolved_at, status
                FROM sensor_alerts
                WHERE sensor_id = $sensorId AND status = 'active'";
            command.Parameters.AddWithValue("$sensorId", sensorId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alerts.Add(new SensorAlert
                {
                    Id = reader.GetInt32(0),
                    SensorId = reader.GetInt32(1),
                    AlertType = reader.GetString(2),
                    Message = reader.GetString(3),
                    Severity = reader.GetString(4),
                    ThresholdValue = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                    MeasuredValue = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                    CreatedAt = reader.GetDateTime(7),
                    ResolvedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    Status = reader.GetString(9)
                });
            }

            return alerts;
        }

        public async Task<List<SensorMaintenanceLog>> GetRecentMaintenanceLogsAsync(int sensorId)
        {
            var logs = new List<SensorMaintenanceLog>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, sensor_id, maintenance_type, performed_by, notes, created_at
                FROM sensor_maintenance_logs
                WHERE sensor_id = $sensorId
                ORDER BY created_at DESC
                LIMIT 5";
            command.Parameters.AddWithValue("$sensorId", sensorId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                logs.Add(new SensorMaintenanceLog
                {
                    Id = reader.GetInt32(0),
                    SensorId = reader.GetInt32(1),
                    MaintenanceType = reader.GetString(2),
                    PerformedBy = reader.GetString(3),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }

            return logs;
        }

        public async Task SaveSensorAsync(Sensor sensor)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO sensors (id, site_id, name, type, unit, location, latitude, longitude, site_type, zone, agglomeration, 
                                     authority, is_active, last_calibration, next_calibration, min_threshold, max_threshold, 
                                     firmware_version, last_maintenance, next_maintenance, status, description, created_at, updated_at)
                VALUES ($id, $site_id, $name, $type, $unit, $location, $latitude, $longitude, $site_type, $zone, $agglomeration, 
                        $authority, $is_active, $last_calibration, $next_calibration, $min_threshold, $max_threshold, 
                        $firmware_version, $last_maintenance, $next_maintenance, $status, $description, $created_at, $updated_at)
                ON CONFLICT(id) DO UPDATE SET
                    site_id = $site_id,
                    name = $name,
                    type = $type,
                    unit = $unit,
                    location = $location,
                    latitude = $latitude,
                    longitude = $longitude,
                    site_type = $site_type,
                    zone = $zone,
                    agglomeration = $agglomeration,
                    authority = $authority,
                    is_active = $is_active,
                    last_calibration = $last_calibration,
                    next_calibration = $next_calibration,
                    min_threshold = $min_threshold,
                    max_threshold = $max_threshold,
                    firmware_version = $firmware_version,
                    last_maintenance = $last_maintenance,
                    next_maintenance = $next_maintenance,
                    status = $status,
                    description = $description,
                    updated_at = $updated_at";
            command.Parameters.AddWithValue("$id", sensor.Id);
            command.Parameters.AddWithValue("$site_id", sensor.SiteId);
            command.Parameters.AddWithValue("$name", sensor.Name);
            command.Parameters.AddWithValue("$type", sensor.Type);
            command.Parameters.AddWithValue("$unit", sensor.Unit);
            command.Parameters.AddWithValue("$location", sensor.Location);
            command.Parameters.AddWithValue("$latitude", sensor.Latitude);
            command.Parameters.AddWithValue("$longitude", sensor.Longitude);
            command.Parameters.AddWithValue("$site_type", sensor.SiteType);
            command.Parameters.AddWithValue("$zone", sensor.Zone);
            command.Parameters.AddWithValue("$agglomeration", sensor.Agglomeration);
            command.Parameters.AddWithValue("$authority", sensor.Authority);
            command.Parameters.AddWithValue("$is_active", sensor.IsActive);
            command.Parameters.AddWithValue("$last_calibration", sensor.LastCalibration);
            command.Parameters.AddWithValue("$next_calibration", sensor.NextCalibration);
            command.Parameters.AddWithValue("$min_threshold", (object?)sensor.MinThreshold ?? DBNull.Value);
            command.Parameters.AddWithValue("$max_threshold", (object?)sensor.MaxThreshold ?? DBNull.Value);
            command.Parameters.AddWithValue("$firmware_version", (object?)sensor.FirmwareVersion ?? DBNull.Value);
            command.Parameters.AddWithValue("$last_maintenance", (object?)sensor.LastMaintenance ?? DBNull.Value);
            command.Parameters.AddWithValue("$next_maintenance", (object?)sensor.NextMaintenance ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", sensor.Status);
            command.Parameters.AddWithValue("$description", (object?)sensor.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$created_at", sensor.CreatedAt);
            command.Parameters.AddWithValue("$updated_at", sensor.UpdatedAt);

            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveSensorAlertAsync(SensorAlert alert)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO sensor_alerts (sensor_id, alert_type, message, severity, threshold_value, measured_value, created_at, resolved_at, status)
                VALUES ($sensor_id, $alert_type, $message, $severity, $threshold_value, $measured_value, $created_at, $resolved_at, $status)";
            command.Parameters.AddWithValue("$sensor_id", alert.SensorId);
            command.Parameters.AddWithValue("$alert_type", alert.AlertType);
            command.Parameters.AddWithValue("$message", alert.Message);
            command.Parameters.AddWithValue("$severity", alert.Severity);
            command.Parameters.AddWithValue("$threshold_value", (object?)alert.ThresholdValue ?? DBNull.Value);
            command.Parameters.AddWithValue("$measured_value", (object?)alert.MeasuredValue ?? DBNull.Value);
            command.Parameters.AddWithValue("$created_at", alert.CreatedAt);
            command.Parameters.AddWithValue("$resolved_at", (object?)alert.ResolvedAt ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", alert.Status);

            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveSensorMaintenanceLogAsync(SensorMaintenanceLog log)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO sensor_maintenance_logs (sensor_id, maintenance_type, performed_by, notes, created_at)
                VALUES ($sensor_id, $maintenance_type, $performed_by, $notes, $created_at)";
            command.Parameters.AddWithValue("$sensor_id", log.SensorId);
            command.Parameters.AddWithValue("$maintenance_type", log.MaintenanceType);
            command.Parameters.AddWithValue("$performed_by", log.PerformedBy);
            command.Parameters.AddWithValue("$notes", (object?)log.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("$created_at", log.CreatedAt);

            await command.ExecuteNonQueryAsync();
        }

        public async Task SaveEnvironmentalDataAsync(EnvironmentalDataEntity data)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO environmental_data (data_type, value, timestamp)
                VALUES ($data_type, $value, $timestamp)";
            command.Parameters.AddWithValue("$data_type", data.DataType);
            command.Parameters.AddWithValue("$value", data.Value);
            command.Parameters.AddWithValue("$timestamp", data.Timestamp);

            await command.ExecuteNonQueryAsync();
        }
    }

    public class EnvironmentalDataEntity
    {
        public int Id { get; set; }
        public string DataType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }
}