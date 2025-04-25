using Microsoft.Data.Sqlite;
using System.Globalization;

namespace SET09102.Services
{
    public class DataImportService
    {
        private readonly DatabaseService _databaseService;
        private readonly string _basePath;

        public DataImportService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public async Task ImportSampleDataAsync()
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    await ImportAirQualityDataAsync(connection);
                    await ImportWaterQualityDataAsync(connection);
                    await ImportWeatherDataAsync(connection);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Error during data import transaction: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ImportSampleDataAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private async Task ImportAirQualityDataAsync(SqliteConnection connection)
        {
            string csvPath = Path.Combine(_basePath, "SampleData", "Air_quality.csv");
            if (!File.Exists(csvPath))
            {
                System.Diagnostics.Debug.WriteLine($"Air quality data file not found at: {csvPath}");
                return;
            }

            try
            {
                // Clear existing data
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM air_quality";
                    await cmd.ExecuteNonQueryAsync();
                }

                // Read and import data
                var lines = await File.ReadAllLinesAsync(csvPath);
                if (lines.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine("Air quality data file is empty or invalid");
                    return;
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO air_quality (
                            date, 
                            time, 
                            nitrogen_dioxide, 
                            sulphur_dioxide, 
                            pm25_particulate_matter, 
                            pm10_particulate_matter
                        ) VALUES (
                            @date, 
                            @time, 
                            @nitrogen_dioxide, 
                            @sulphur_dioxide, 
                            @pm25, 
                            @pm10
                        )";

                    var dateParam = cmd.CreateParameter();
                    var timeParam = cmd.CreateParameter();
                    var no2Param = cmd.CreateParameter();
                    var so2Param = cmd.CreateParameter();
                    var pm25Param = cmd.CreateParameter();
                    var pm10Param = cmd.CreateParameter();

                    cmd.Parameters.AddRange(new[] { dateParam, timeParam, no2Param, so2Param, pm25Param, pm10Param });

                    dateParam.ParameterName = "@date";
                    timeParam.ParameterName = "@time";
                    no2Param.ParameterName = "@nitrogen_dioxide";
                    so2Param.ParameterName = "@sulphur_dioxide";
                    pm25Param.ParameterName = "@pm25";
                    pm10Param.ParameterName = "@pm10";

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',');
                        if (values.Length >= 6)
                        {
                            dateParam.Value = values[0];
                            timeParam.Value = values[1];
                            no2Param.Value = ParseNullableDouble(values[2]);
                            so2Param.Value = ParseNullableDouble(values[3]);
                            pm25Param.Value = ParseNullableDouble(values[4]);
                            pm10Param.Value = ParseNullableDouble(values[5]);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing air quality data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private async Task ImportWaterQualityDataAsync(SqliteConnection connection)
        {
            string csvPath = Path.Combine(_basePath, "SampleData", "Water_quality.csv");
            if (!File.Exists(csvPath))
            {
                System.Diagnostics.Debug.WriteLine($"Water quality data file not found at: {csvPath}");
                return;
            }

            try
            {
                // Clear existing data
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM water_quality";
                    await cmd.ExecuteNonQueryAsync();
                }

                // Read and import data
                var lines = await File.ReadAllLinesAsync(csvPath);
                if (lines.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine("Water quality data file is empty or invalid");
                    return;
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO water_quality (
                            date, 
                            time, 
                            nitrate, 
                            nitrite, 
                            phosphate, 
                            ec_cfu_per_100ml
                        ) VALUES (
                            @date, 
                            @time, 
                            @nitrate, 
                            @nitrite, 
                            @phosphate, 
                            @ec
                        )";

                    var dateParam = cmd.CreateParameter();
                    var timeParam = cmd.CreateParameter();
                    var nitrateParam = cmd.CreateParameter();
                    var nitriteParam = cmd.CreateParameter();
                    var phosphateParam = cmd.CreateParameter();
                    var ecParam = cmd.CreateParameter();

                    cmd.Parameters.AddRange(new[] { dateParam, timeParam, nitrateParam, nitriteParam, phosphateParam, ecParam });

                    dateParam.ParameterName = "@date";
                    timeParam.ParameterName = "@time";
                    nitrateParam.ParameterName = "@nitrate";
                    nitriteParam.ParameterName = "@nitrite";
                    phosphateParam.ParameterName = "@phosphate";
                    ecParam.ParameterName = "@ec";

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',');
                        if (values.Length >= 6)
                        {
                            dateParam.Value = values[0];
                            timeParam.Value = values[1];
                            nitrateParam.Value = ParseNullableDouble(values[2]);
                            nitriteParam.Value = ParseNullableDouble(values[3]);
                            phosphateParam.Value = ParseNullableDouble(values[4]);
                            ecParam.Value = ParseNullableDouble(values[5]);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing water quality data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private async Task ImportWeatherDataAsync(SqliteConnection connection)
        {
            string csvPath = Path.Combine(_basePath, "SampleData", "Weather.csv");
            if (!File.Exists(csvPath))
            {
                System.Diagnostics.Debug.WriteLine($"Weather data file not found at: {csvPath}");
                return;
            }

            try
            {
                // Clear existing data
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM weather_conditions";
                    await cmd.ExecuteNonQueryAsync();
                }

                // Read and import data
                var lines = await File.ReadAllLinesAsync(csvPath);
                if (lines.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine("Weather data file is empty or invalid");
                    return;
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO weather_conditions (
                            time, 
                            temperature_2m, 
                            relative_humidity_2m, 
                            wind_speed_10m, 
                            wind_direction_10m
                        ) VALUES (
                            @time, 
                            @temperature, 
                            @humidity, 
                            @wind_speed, 
                            @wind_direction
                        )";

                    var timeParam = cmd.CreateParameter();
                    var tempParam = cmd.CreateParameter();
                    var humidityParam = cmd.CreateParameter();
                    var windSpeedParam = cmd.CreateParameter();
                    var windDirParam = cmd.CreateParameter();

                    cmd.Parameters.AddRange(new[] { timeParam, tempParam, humidityParam, windSpeedParam, windDirParam });

                    timeParam.ParameterName = "@time";
                    tempParam.ParameterName = "@temperature";
                    humidityParam.ParameterName = "@humidity";
                    windSpeedParam.ParameterName = "@wind_speed";
                    windDirParam.ParameterName = "@wind_direction";

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',');
                        if (values.Length >= 5)
                        {
                            timeParam.Value = values[0];
                            tempParam.Value = ParseNullableDouble(values[1]);
                            humidityParam.Value = ParseNullableDouble(values[2]);
                            windSpeedParam.Value = ParseNullableDouble(values[3]);
                            windDirParam.Value = ParseNullableDouble(values[4]);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing weather data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private object ParseNullableDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            return DBNull.Value;
        }
    }
} 