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
                // Get the application data directory
                string appDataPath = Path.Combine(FileSystem.AppDataDirectory, "EnvironmentalMonitoring");
                
                // Create the directory if it doesn't exist
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                // Set the database path
                _dbPath = Path.Combine(appDataPath, "environmental_monitoring.db");
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database paths", ex);
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Check if database already exists
                if (File.Exists(_dbPath))
                {
                    if (await VerifySchemaAsync())
                    {
                        return;
                    }
                    else
                    {
                        File.Delete(_dbPath);
                    }
                }

                // Create the database file
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                await _connection.OpenAsync();

                // Create tables directly
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
                    );
                    
                    CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL UNIQUE,
                        password_hash TEXT NOT NULL,
                        role TEXT NOT NULL,
                        full_name TEXT NOT NULL,
                        email TEXT,
                        is_active INTEGER NOT NULL DEFAULT 1,
                        created_at TEXT NOT NULL,
                        last_login TEXT
                    );";

                using (var command = new SqliteCommand(createTablesSql, _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Initialize sensors
                var sensorService = new SensorService(_dbPath);
                await sensorService.InitializeSensorsAsync();
                
                // Initialize users
                var userService = new UserService(this);
                await userService.InitializeUsersTableAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database", ex);
            }
        }

        private async Task<bool> VerifySchemaAsync()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    await connection.OpenAsync();

                    // Check if all required tables exist
                    var tables = new[] { 
                        "air_quality", 
                        "water_quality", 
                        "weather_conditions", 
                        "sensors", 
                        "sensor_alerts", 
                        "sensor_maintenance_logs", 
                        "environmental_data",
                        "users"
                    };
                    
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

        public string GetDatabasePath()
        {
            return _dbPath;
        }

        public SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                _connection.Open();
            }
            return _connection;
        }

        // Get environmental data from database
        public async Task<List<EnvironmentalDataEntity>> GetEnvironmentalDataAsync()
        {
            var data = new List<EnvironmentalDataEntity>();
            
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT data_type, value, timestamp FROM environmental_data";
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                data.Add(new EnvironmentalDataEntity
                {
                    DataType = reader.GetString(0),
                    Value = reader.GetDouble(1),
                    Timestamp = reader.GetString(2)
                });
            }
            
            return data;
        }
        
        // Add method to save environmental data
        public async Task SaveEnvironmentalDataAsync(EnvironmentalDataEntity data)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO environmental_data (data_type, value, timestamp)
                VALUES ($dataType, $value, $timestamp)";
            
            command.Parameters.AddWithValue("$dataType", data.DataType);
            command.Parameters.AddWithValue("$value", data.Value);
            command.Parameters.AddWithValue("$timestamp", data.Timestamp);
            
            await command.ExecuteNonQueryAsync();
        }
    }
}