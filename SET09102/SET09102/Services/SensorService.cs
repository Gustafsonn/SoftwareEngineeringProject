using SET09102.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace SET09102.Services;

public interface ISensorService
{
    Task<ObservableCollection<Sensor>> GetSensorsAsync();  
    Task UpdateSensorAsync(Sensor sensor);
    Task<ObservableCollection<Malfunction>> GetMalfunctionsAsync(int sensorId);
    Task CreateMalfunctionAsync(Malfunction malfunction);
    Task UpdateMalfunctionAsync(Malfunction malfunction);
}

public class SensorService : ISensorService
{
    private readonly string _dbPath;

    public SensorService(string dbPath)
    {
        _dbPath = dbPath;
    }

    public async Task InitializeSensorsAsync()
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            // Air Quality Sensors - Edinburgh Nicolson Street
            var airSensors = new List<Sensor>
            {
                new Sensor
                {
                    SiteId = 1,
                    Name = "Nitrogen Dioxide Sensor",
                    Type = "Air Quality",
                    Unit = "µg/m³",
                    Location = "Edinburgh Nicolson Street",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Urban Traffic",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-30),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 200,
                    FirmwareVersion = "2.1.0",
                    LastMaintenance = DateTime.Now.AddDays(-60),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures NO2 levels for air quality monitoring at Edinburgh Nicolson Street"
                },
                new Sensor
                {
                    SiteId = 1,
                    Name = "Sulphur Dioxide Sensor",
                    Type = "Air Quality",
                    Unit = "µg/m³",
                    Location = "Edinburgh Nicolson Street",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Urban Traffic",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-45),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 350,
                    FirmwareVersion = "2.1.0",
                    LastMaintenance = DateTime.Now.AddDays(-45),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures SO2 levels for air quality monitoring at Edinburgh Nicolson Street"
                },
                new Sensor
                {
                    SiteId = 1,
                    Name = "PM2.5 Sensor",
                    Type = "Air Quality",
                    Unit = "µg/m³",
                    Location = "Edinburgh Nicolson Street",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Urban Traffic",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-15),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 25,
                    FirmwareVersion = "2.1.0",
                    LastMaintenance = DateTime.Now.AddDays(-30),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures fine particulate matter (PM2.5) at Edinburgh Nicolson Street"
                },
                new Sensor
                {
                    SiteId = 1,
                    Name = "PM10 Sensor",
                    Type = "Air Quality",
                    Unit = "µg/m³",
                    Location = "Edinburgh Nicolson Street",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Urban Traffic",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-20),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 50,
                    FirmwareVersion = "2.1.0",
                    LastMaintenance = DateTime.Now.AddDays(-25),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures coarse particulate matter (PM10) at Edinburgh Nicolson Street"
                }
            };

            // Water Quality Sensors - Water Quality Sensor 1
            var waterSensors = new List<Sensor>
            {
                new Sensor
                {
                    SiteId = 2,
                    Name = "Nitrate Sensor",
                    Type = "Water Quality",
                    Unit = "mg/l",
                    Location = "Water Quality Sensor 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Water Quality",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-10),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 50,
                    FirmwareVersion = "1.8.0",
                    LastMaintenance = DateTime.Now.AddDays(-20),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures nitrate levels in water at Water Quality Sensor 1"
                },
                new Sensor
                {
                    SiteId = 2,
                    Name = "Nitrite Sensor",
                    Type = "Water Quality",
                    Unit = "mg/l",
                    Location = "Water Quality Sensor 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Water Quality",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-5),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 3,
                    FirmwareVersion = "1.8.0",
                    LastMaintenance = DateTime.Now.AddDays(-15),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures nitrite levels in water at Water Quality Sensor 1"
                },
                new Sensor
                {
                    SiteId = 2,
                    Name = "Phosphate Sensor",
                    Type = "Water Quality",
                    Unit = "mg/l",
                    Location = "Water Quality Sensor 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Water Quality",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-8),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 0.1,
                    FirmwareVersion = "1.8.0",
                    LastMaintenance = DateTime.Now.AddDays(-18),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures phosphate levels in water at Water Quality Sensor 1"
                },
                new Sensor
                {
                    SiteId = 2,
                    Name = "E. Coli Sensor",
                    Type = "Water Quality",
                    Unit = "cfu/100ml",
                    Location = "Water Quality Sensor 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Water Quality",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-12),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 100,
                    FirmwareVersion = "1.8.0",
                    LastMaintenance = DateTime.Now.AddDays(-22),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures E. Coli levels in water at Water Quality Sensor 1"
                }
            };

            // Weather Sensors - Weather Station 1
            var weatherSensors = new List<Sensor>
            {
                new Sensor
                {
                    SiteId = 3,
                    Name = "Temperature Sensor",
                    Type = "Weather",
                    Unit = "°C",
                    Location = "Weather Station 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Weather",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-7),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = -20,
                    MaxThreshold = 40,
                    FirmwareVersion = "3.0.0",
                    LastMaintenance = DateTime.Now.AddDays(-14),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures ambient temperature at 2m height at Weather Station 1"
                },
                new Sensor
                {
                    SiteId = 3,
                    Name = "Humidity Sensor",
                    Type = "Weather",
                    Unit = "%",
                    Location = "Weather Station 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Weather",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-9),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 100,
                    FirmwareVersion = "3.0.0",
                    LastMaintenance = DateTime.Now.AddDays(-16),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures relative humidity at 2m height at Weather Station 1"
                },
                new Sensor
                {
                    SiteId = 3,
                    Name = "Wind Speed Sensor",
                    Type = "Weather",
                    Unit = "m/s",
                    Location = "Weather Station 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Weather",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-5),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 50,
                    FirmwareVersion = "3.0.0",
                    LastMaintenance = DateTime.Now.AddDays(-12),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures wind speed at 10m height at Weather Station 1"
                },
                new Sensor
                {
                    SiteId = 3,
                    Name = "Wind Direction Sensor",
                    Type = "Weather",
                    Unit = "degrees",
                    Location = "Weather Station 1",
                    Latitude = 55.94476,
                    Longitude = -3.183991,
                    SiteType = "Weather",
                    Zone = "Central Scotland",
                    Agglomeration = "Edinburgh Urban Area",
                    Authority = "Edinburgh",
                    IsActive = true,
                    LastCalibration = DateTime.Now.AddDays(-6),
                    NextCalibration = DateTime.Now.AddMonths(3),
                    MinThreshold = 0,
                    MaxThreshold = 360,
                    FirmwareVersion = "3.0.0",
                    LastMaintenance = DateTime.Now.AddDays(-13),
                    NextMaintenance = DateTime.Now.AddMonths(6),
                    Status = "operational",
                    Description = "Measures wind direction at 10m height at Weather Station 1"
                }
            };

            // Insert all sensors into database
            foreach (var sensor in airSensors.Concat(waterSensors).Concat(weatherSensors))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO sensors (
                        site_id, name, type, unit, location, latitude, longitude,
                        site_type, zone, agglomeration, authority, is_active,
                        last_calibration, next_calibration, min_threshold, max_threshold,
                        firmware_version, last_maintenance, next_maintenance, status, description
                    ) VALUES (
                        $siteId, $name, $type, $unit, $location, $latitude, $longitude,
                        $siteType, $zone, $agglomeration, $authority, $isActive,
                        $lastCalibration, $nextCalibration, $minThreshold, $maxThreshold,
                        $firmwareVersion, $lastMaintenance, $nextMaintenance, $status, $description
                    )";

                command.Parameters.AddWithValue("$siteId", sensor.SiteId);
                command.Parameters.AddWithValue("$name", sensor.Name);
                command.Parameters.AddWithValue("$type", sensor.Type);
                command.Parameters.AddWithValue("$unit", sensor.Unit);
                command.Parameters.AddWithValue("$location", sensor.Location);
                command.Parameters.AddWithValue("$latitude", sensor.Latitude);
                command.Parameters.AddWithValue("$longitude", sensor.Longitude);
                command.Parameters.AddWithValue("$siteType", sensor.SiteType);
                command.Parameters.AddWithValue("$zone", sensor.Zone);
                command.Parameters.AddWithValue("$agglomeration", sensor.Agglomeration);
                command.Parameters.AddWithValue("$authority", sensor.Authority);
                command.Parameters.AddWithValue("$isActive", sensor.IsActive);
                command.Parameters.AddWithValue("$lastCalibration", sensor.LastCalibration);
                command.Parameters.AddWithValue("$nextCalibration", sensor.NextCalibration);
                command.Parameters.AddWithValue("$minThreshold", sensor.MinThreshold);
                command.Parameters.AddWithValue("$maxThreshold", sensor.MaxThreshold);
                command.Parameters.AddWithValue("$firmwareVersion", sensor.FirmwareVersion);
                command.Parameters.AddWithValue("$lastMaintenance", sensor.LastMaintenance);
                command.Parameters.AddWithValue("$nextMaintenance", sensor.NextMaintenance);
                command.Parameters.AddWithValue("$status", sensor.Status);
                command.Parameters.AddWithValue("$description", sensor.Description);

                await command.ExecuteNonQueryAsync();
            }

            // Create some example sensor malfunctions.
            var malfuncions = new List<Malfunction> {
                new ()
                {
                    SensorId = 1,
                    Description = "This is an example of an active (unresolved) malfunction for a sensor",
                    Resolved = false
                },
                new ()
                {
                    SensorId = 2,
                    Description = "This is an example of an inactive (resolved) malfunction for a sensor",
                    Resolved = true
                }
            };

            // Insert all the example sensor malfunctions into database.
            foreach (var malfunction in malfuncions)
            {
                await CreateMalfunctionAsync(malfunction);                
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ObservableCollection<Sensor>> GetSensorsAsync()
    {
        var sensors = new ObservableCollection<Sensor>();
        try
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM sensors";

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
                    FirmwareVersion = reader.GetString(17),
                    LastMaintenance = reader.IsDBNull(18) ? null : reader.GetDateTime(18),
                    NextMaintenance = reader.IsDBNull(19) ? null : reader.GetDateTime(19),
                    Status = reader.GetString(20),
                    Description = reader.GetString(21)
                });
            }
        }
        catch (Exception)
        {
            throw;
        }

        return sensors;
    }
 

    public async Task UpdateSensorAsync(Sensor sensor)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
        UPDATE sensors SET
            site_id = $siteId,
            name = $name,
            type = $type,
            unit = $unit,
            location = $location,
            latitude = $latitude,
            longitude = $longitude,
            site_type = $siteType,
            zone = $zone,
            agglomeration = $agglomeration,
            authority = $authority,
            is_active = $isActive,
            last_calibration = $lastCalibration,
            next_calibration = $nextCalibration,
            min_threshold = $minThreshold,
            max_threshold = $maxThreshold,
            firmware_version = $firmwareVersion,
            last_maintenance = $lastMaintenance,
            next_maintenance = $nextMaintenance,
            status = $status,
            description = $description,
            updated_at = $updatedAt
        WHERE id = $id;";

        command.Parameters.AddWithValue("$id", sensor.Id);
        command.Parameters.AddWithValue("$siteId", sensor.SiteId);
        command.Parameters.AddWithValue("$name", sensor.Name);
        command.Parameters.AddWithValue("$type", sensor.Type);
        command.Parameters.AddWithValue("$unit", sensor.Unit);
        command.Parameters.AddWithValue("$location", sensor.Location);
        command.Parameters.AddWithValue("$latitude", sensor.Latitude);
        command.Parameters.AddWithValue("$longitude", sensor.Longitude);
        command.Parameters.AddWithValue("$siteType", sensor.SiteType);
        command.Parameters.AddWithValue("$zone", sensor.Zone);
        command.Parameters.AddWithValue("$agglomeration", sensor.Agglomeration);
        command.Parameters.AddWithValue("$authority", sensor.Authority);
        command.Parameters.AddWithValue("$isActive", sensor.IsActive);
        command.Parameters.AddWithValue("$lastCalibration", sensor.LastCalibration);
        command.Parameters.AddWithValue("$nextCalibration", sensor.NextCalibration);
        command.Parameters.AddWithValue("$minThreshold", sensor.MinThreshold ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$maxThreshold", sensor.MaxThreshold ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$firmwareVersion", sensor.FirmwareVersion);
        command.Parameters.AddWithValue("$lastMaintenance", sensor.LastMaintenance ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$nextMaintenance", sensor.NextMaintenance ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$status", sensor.Status);
        command.Parameters.AddWithValue("$description", sensor.Description);
        command.Parameters.AddWithValue("$updatedAt", sensor.UpdatedAt);

        await command.ExecuteNonQueryAsync();
    }


    public async Task<ObservableCollection<Malfunction>> GetMalfunctionsAsync(int sensorId)
    {
        var malfunctions = new ObservableCollection<Malfunction>();

        try
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM malfunctions WHERE sensor_id=$sensor_id";
            command.Parameters.AddWithValue("$sensor_id", sensorId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                malfunctions.Add(new Malfunction
                {
                    Id = reader.GetInt32(0),
                    SensorId = reader.GetInt32(1),
                    Description = reader.GetString(2),
                    Resolved = reader.GetBoolean(3)
                });
            }
        }
        catch (Exception)
        {
            throw;
        }

        return malfunctions;
    }

    public async Task CreateMalfunctionAsync(Malfunction malfunction)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO malfunctions (sensor_id, description, resolved)
            VALUES ($sensor_id, $description, $resolved);";

        command.Parameters.AddWithValue("$id", malfunction.Id);
        command.Parameters.AddWithValue("$sensor_id", malfunction.SensorId);
        command.Parameters.AddWithValue("$description", malfunction.Description);
        command.Parameters.AddWithValue("$resolved", malfunction.Resolved);
        await command.ExecuteNonQueryAsync();

    }

    public async Task UpdateMalfunctionAsync(Malfunction malfunction)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE malfunctions SET
                sensor_id = $sensor_id,
                description = $description,
                resolved = $resolved
            WHERE id = $id;";

        command.Parameters.AddWithValue("$id", malfunction.Id);
        command.Parameters.AddWithValue("$sensor_id", malfunction.SensorId);
        command.Parameters.AddWithValue("$description", malfunction.Description);
        command.Parameters.AddWithValue("$resolved", malfunction.Resolved);
        await command.ExecuteNonQueryAsync();
    }
} 