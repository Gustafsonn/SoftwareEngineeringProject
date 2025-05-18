using Microsoft.Data.Sqlite;
using SET09102.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SET09102.Services
{
    /// <summary>
    /// Provides core database functionality for the application, including database initialization,
    /// connection management, and basic data operations.
    /// </summary>
    /// <remarks>
    /// This service is responsible for:
    /// - Setting up the database file path and connection string
    /// - Creating database schema during initialization
    /// - Verifying database structure
    /// - Providing connection objects to other services
    /// - Retrieving and storing environmental data
    /// </remarks>
    public class DatabaseService
    {
        private readonly string _dbPath;
        private SqliteConnection? _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class.
        /// Sets up the database file path in the application data directory.
        /// </summary>
        /// <exception cref="Exception">Thrown when there is an error setting up the database paths.</exception>
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

        /// <summary>
        /// Initializes the database by creating necessary tables if they don't exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error initializing the database.</exception>
        public async Task InitializeAsync()
        {
            try
            {
                // Check if database already exists and its schema is valid
                if (File.Exists(_dbPath))
                {
                    if (await VerifySchemaAsync())
                    {
                        return;
                    }
                    else
                    {
                        // Delete existing database if schema is invalid
                        File.Delete(_dbPath);
                    }
                }

                // Create the database file and open connection
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                await _connection.OpenAsync();

                // Define schema for database tables
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


                    CREATE TABLE IF NOT EXISTS malfunctions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        sensor_id INTEGER NOT NULL,
                        description TEXT NOT NULL,
                        resolved BOOLEAN NOT NULL DEFAULT 0,
                        FOREIGN KEY (sensor_id) REFERENCES sensors(id)
                    );
                    
                    CREATE TABLE IF NOT EXISTS maintenance_logs (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        sensor_id INTEGER NOT NULL,
                        maintenance_type TEXT NOT NULL,
                        performed_by TEXT NOT NULL,
                        notes TEXT,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (sensor_id) REFERENCES sensors(id)
                    );";

                // Execute the SQL to create tables
                using (var command = new SqliteCommand(createTablesSql, _connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Initialize sensors with default data
                var sensorService = new SensorService(_dbPath);
                await sensorService.InitializeSensorsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing database", ex);
            }
        }

        /// <summary>
        /// Verifies that the database schema is valid by checking for required tables.
        /// </summary>
        /// <returns>True if the schema is valid; otherwise, false.</returns>
        private async Task<bool> VerifySchemaAsync()
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    await connection.OpenAsync();

                    // Check if all required tables exist
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

        /// <summary>
        /// Gets the full path to the database file.
        /// </summary>
        /// <returns>The database file path.</returns>
        public string GetDatabasePath()
        {
            return _dbPath;
        }

        /// <summary>
        /// Gets an open connection to the database.
        /// If the connection doesn't exist or is closed, a new one is created and opened.
        /// </summary>
        /// <returns>An open SQLite connection.</returns>
        public SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                _connection.Open();
            }
            return _connection;
        }

        /// <summary>
        /// Retrieves all environmental data from the database.
        /// </summary>
        /// <returns>A list of environmental data entities.</returns>
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
        
        /// <summary>
        /// Saves environmental data to the database.
        /// </summary>
        /// <param name="data">The environmental data to save.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

// EnvironmentalDataEntity.cs
namespace SET09102.Models
{
    /// <summary>
    /// Represents a single environmental data point stored in the database.
    /// </summary>
    /// <remarks>
    /// This class is used for database operations and is distinct from the ViewModel
    /// classes that might use this data for presentation.
    /// </remarks>
    public class EnvironmentalDataEntity
    {
        /// <summary>
        /// Gets or sets the type of environmental data (e.g., temperature, humidity).
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the numerical value of the data point.
        /// </summary>
        public double Value { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the data was recorded.
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;
    }
}