using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using SET09102.Services;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace SET09102.Models
{
    public class HistoricalDataViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<EnvironmentalDataPoint> _historicalData = new();
        private bool _isLoading;
        private bool _hasData;
        private string _currentMetric = "Value";
        private double _average;
        private double _minimum;
        private double _maximum;
        private int _dataPoints;
        private string _trendMessage = "No trend data available";
        private string _trendDetail = "Select data to view trend analysis";
        private double? _currentThreshold;
        private string _selectedDataType = string.Empty;

        public ObservableCollection<EnvironmentalDataPoint> HistoricalData
        {
            get => _historicalData;
            set
            {
                _historicalData = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public bool HasData
        {
            get => _hasData;
            set
            {
                _hasData = value;
                OnPropertyChanged();
            }
        }
        
        public string CurrentMetric
        {
            get => _currentMetric;
            set
            {
                _currentMetric = value;
                OnPropertyChanged();
            }
        }
        
        public double Average
        {
            get => _average;
            set
            {
                _average = value;
                OnPropertyChanged();
            }
        }
        
        public double Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                OnPropertyChanged();
            }
        }
        
        public double Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                OnPropertyChanged();
            }
        }
        
        public int DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                OnPropertyChanged();
            }
        }
        
        public string TrendMessage
        {
            get => _trendMessage;
            set
            {
                _trendMessage = value;
                OnPropertyChanged();
            }
        }
        
        public string TrendDetail
        {
            get => _trendDetail;
            set
            {
                _trendDetail = value;
                OnPropertyChanged();
            }
        }
        
        public double? CurrentThreshold
        {
            get => _currentThreshold;
            set
            {
                _currentThreshold = value;
                OnPropertyChanged();
            }
        }
        
        public HistoricalDataViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            HistoricalData = new ObservableCollection<EnvironmentalDataPoint>();
            HasData = false;
        }

        public async Task LoadDataAsync(string dataType, DateTime startDate, DateTime endDate)
        {
            try
            {
                IsLoading = true;
                _selectedDataType = dataType;
                
                HistoricalData.Clear();
                
                // Get data from database service based on data type
                switch (dataType)
                {
                    case "Air Quality":
                        await LoadAirQualityDataAsync(startDate, endDate);
                        break;
                    case "Water Quality":
                        await LoadWaterQualityDataAsync(startDate, endDate);
                        break;
                    case "Weather":
                        await LoadWeatherDataAsync(startDate, endDate);
                        break;
                }
                
                HasData = HistoricalData.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading historical data: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                if (Application.Current != null && Application.Current.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Error", "Failed to load data. Please try again.", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadAirQualityDataAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.GetConnection();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    date || ' ' || time as datetime,
                    'Edinburgh Nicolson Street' as location,
                    nitrogen_dioxide, 
                    sulphur_dioxide, 
                    pm25_particulate_matter, 
                    pm10_particulate_matter
                FROM 
                    air_quality
                WHERE 
                    date >= @StartDate AND date <= @EndDate
                ORDER BY 
                    date, time";
                    
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("dd/MM/yyyy"));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("dd/MM/yyyy"));
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                DateTime timestamp = DateTime.Parse(reader.GetString(0));
                string location = reader.GetString(1);
                
                // Add nitrogen dioxide data point
                if (!reader.IsDBNull(2))
                {
                    var no2Value = reader.GetDouble(2);
                    string status = GetAirQualityStatus("Nitrogen Dioxide", no2Value);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Air Quality",
                        SensorName = "Nitrogen Dioxide Sensor",
                        Value = no2Value,
                        Unit = "µg/m³",
                        Status = status
                    });
                }
                
                // Add sulphur dioxide data point
                if (!reader.IsDBNull(3))
                {
                    var so2Value = reader.GetDouble(3);
                    string status = GetAirQualityStatus("Sulphur Dioxide", so2Value);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Air Quality",
                        SensorName = "Sulphur Dioxide Sensor",
                        Value = so2Value,
                        Unit = "µg/m³",
                        Status = status
                    });
                }
                
                // Add PM2.5 data point
                if (!reader.IsDBNull(4))
                {
                    var pm25Value = reader.GetDouble(4);
                    string status = GetAirQualityStatus("PM2.5 Particulate Matter", pm25Value);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Air Quality",
                        SensorName = "PM2.5 Sensor",
                        Value = pm25Value,
                        Unit = "µg/m³",
                        Status = status
                    });
                }
                
                // Add PM10 data point
                if (!reader.IsDBNull(5))
                {
                    var pm10Value = reader.GetDouble(5);
                    string status = GetAirQualityStatus("PM10 Particulate Matter", pm10Value);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Air Quality",
                        SensorName = "PM10 Sensor",
                        Value = pm10Value,
                        Unit = "µg/m³",
                        Status = status
                    });
                }
            }
        }
        
        private async Task LoadWaterQualityDataAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.GetConnection();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    date || ' ' || time as datetime,
                    'Water Quality Sensor 1' as location,
                    nitrate, 
                    nitrite, 
                    phosphate, 
                    ec_cfu_per_100ml
                FROM 
                    water_quality
                WHERE 
                    date >= @StartDate AND date <= @EndDate
                ORDER BY 
                    date, time";
                    
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("dd/MM/yyyy"));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("dd/MM/yyyy"));
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                DateTime timestamp = DateTime.Parse(reader.GetString(0));
                string location = reader.GetString(1);
                
                // Add nitrate data point
                if (!reader.IsDBNull(2))
                {
                    var nitrateValue = reader.GetDouble(2);
                    string status = GetWaterQualityStatus("Nitrate", nitrateValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Water Quality",
                        SensorName = "Nitrate Sensor",
                        Value = nitrateValue,
                        Unit = "mg/l",
                        Status = status
                    });
                }
                
                // Add nitrite data point
                if (!reader.IsDBNull(3))
                {
                    var nitriteValue = reader.GetDouble(3);
                    string status = GetWaterQualityStatus("Nitrite", nitriteValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Water Quality",
                        SensorName = "Nitrite Sensor",
                        Value = nitriteValue,
                        Unit = "mg/l",
                        Status = status
                    });
                }
                
                // Add phosphate data point
                if (!reader.IsDBNull(4))
                {
                    var phosphateValue = reader.GetDouble(4);
                    string status = GetWaterQualityStatus("Phosphate", phosphateValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Water Quality",
                        SensorName = "Phosphate Sensor",
                        Value = phosphateValue,
                        Unit = "mg/l",
                        Status = status
                    });
                }
                
                // Add e.coli data point
                if (!reader.IsDBNull(5))
                {
                    var ecoliValue = reader.GetDouble(5);
                    string status = GetWaterQualityStatus("E. Coli", ecoliValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Water Quality",
                        SensorName = "E. Coli Sensor",
                        Value = ecoliValue,
                        Unit = "cfu/100ml",
                        Status = status
                    });
                }
            }
        }
        
        private async Task LoadWeatherDataAsync(DateTime startDate, DateTime endDate)
        {
            using var connection = _databaseService.GetConnection();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    time,
                    'Weather Station 1' as location,
                    temperature_2m, 
                    relative_humidity_2m, 
                    wind_speed_10m, 
                    wind_direction_10m
                FROM 
                    weather_conditions
                WHERE 
                    time >= @StartDate AND time <= @EndDate
                ORDER BY 
                    time";
                    
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                DateTime timestamp = DateTime.Parse(reader.GetString(0));
                string location = reader.GetString(1);
                
                // Add temperature data point
                if (!reader.IsDBNull(2))
                {
                    var temperatureValue = reader.GetDouble(2);
                    string status = GetWeatherStatus("Temperature", temperatureValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Weather",
                        SensorName = "Temperature Sensor",
                        Value = temperatureValue,
                        Unit = "°C",
                        Status = status
                    });
                }
                
                // Add humidity data point
                if (!reader.IsDBNull(3))
                {
                    var humidityValue = reader.GetDouble(3);
                    string status = GetWeatherStatus("Relative Humidity", humidityValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Weather",
                        SensorName = "Humidity Sensor",
                        Value = humidityValue,
                        Unit = "%",
                        Status = status
                    });
                }
                
                // Add wind speed data point
                if (!reader.IsDBNull(4))
                {
                    var windSpeedValue = reader.GetDouble(4);
                    string status = GetWeatherStatus("Wind Speed", windSpeedValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Weather",
                        SensorName = "Wind Speed Sensor",
                        Value = windSpeedValue,
                        Unit = "m/s",
                        Status = status
                    });
                }
                
                // Add wind direction data point
                if (!reader.IsDBNull(5))
                {
                    var windDirectionValue = reader.GetDouble(5);
                    string status = GetWeatherStatus("Wind Direction", windDirectionValue);
                    HistoricalData.Add(new EnvironmentalDataPoint
                    {
                        Timestamp = timestamp,
                        Location = location,
                        DataType = "Weather",
                        SensorName = "Wind Direction Sensor",
                        Value = windDirectionValue,
                        Unit = "°",
                        Status = status
                    });
                }
            }
        }
        
        public async Task UpdateSelectedMetricAsync(string metric)
        {
            if (string.IsNullOrEmpty(metric) || HistoricalData.Count == 0)
                return;
            
            CurrentMetric = metric;
            
            // Filter data by the selected metric
            var filteredData = new List<EnvironmentalDataPoint>();
            
            foreach (var dataPoint in HistoricalData)
            {
                if (_selectedDataType == "Air Quality")
                {
                    if ((metric == "Nitrogen Dioxide" && dataPoint.SensorName == "Nitrogen Dioxide Sensor") ||
                        (metric == "Sulphur Dioxide" && dataPoint.SensorName == "Sulphur Dioxide Sensor") ||
                        (metric == "PM2.5 Particulate Matter" && dataPoint.SensorName == "PM2.5 Sensor") ||
                        (metric == "PM10 Particulate Matter" && dataPoint.SensorName == "PM10 Sensor"))
                    {
                        filteredData.Add(dataPoint);
                    }
                }
                else if (_selectedDataType == "Water Quality")
                {
                    if ((metric == "Nitrate" && dataPoint.SensorName == "Nitrate Sensor") ||
                        (metric == "Nitrite" && dataPoint.SensorName == "Nitrite Sensor") ||
                        (metric == "Phosphate" && dataPoint.SensorName == "Phosphate Sensor") ||
                        (metric == "E. Coli" && dataPoint.SensorName == "E. Coli Sensor"))
                    {
                        filteredData.Add(dataPoint);
                    }
                }
                else if (_selectedDataType == "Weather")
                {
                    if ((metric == "Temperature" && dataPoint.SensorName == "Temperature Sensor") ||
                        (metric == "Relative Humidity" && dataPoint.SensorName == "Humidity Sensor") ||
                        (metric == "Wind Speed" && dataPoint.SensorName == "Wind Speed Sensor") ||
                        (metric == "Wind Direction" && dataPoint.SensorName == "Wind Direction Sensor"))
                    {
                        filteredData.Add(dataPoint);
                    }
                }
            }
            
            // Calculate statistics
            if (filteredData.Count > 0)
            {
                var values = filteredData.Select(d => d.Value).ToList();
                
                Average = values.Average();
                Minimum = values.Min();
                Maximum = values.Max();
                DataPoints = values.Count;
                
                // Set threshold based on metric
                CurrentThreshold = GetThresholdForMetric(metric);
                
                // Analyze trend
                AnalyzeTrend(filteredData);
            }
            else
            {
                Average = 0;
                Minimum = 0;
                Maximum = 0;
                DataPoints = 0;
                CurrentThreshold = null;
                TrendMessage = "No data available for selected metric";
                TrendDetail = "Please select a different metric or date range";
            }
        }
        
        private void AnalyzeTrend(List<EnvironmentalDataPoint> data)
        {
            if (data.Count < 2)
            {
                TrendMessage = "Insufficient data for trend analysis";
                TrendDetail = "More data points needed to determine a trend";
                return;
            }
            
            // Sort data by timestamp
            var sortedData = data.OrderBy(d => d.Timestamp).ToList();
            
            // Calculate simple linear regression
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = sortedData.Count;
            
            // Use relative time in hours from first reading
            DateTime firstTimestamp = sortedData.First().Timestamp;
            
            for (int i = 0; i < n; i++)
            {
                double x = (sortedData[i].Timestamp - firstTimestamp).TotalHours;
                double y = sortedData[i].Value;
                
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }
            
            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            
            // Check if values are exceeding thresholds
            int exceedingThreshold = 0;
            if (CurrentThreshold.HasValue)
            {
                exceedingThreshold = sortedData.Count(d => d.Value > CurrentThreshold.Value);
            }
            
            // First and last values for comparison
            double firstValue = sortedData.First().Value;
            double lastValue = sortedData.Last().Value;
            double percentChange = 100 * (lastValue - firstValue) / firstValue;
            
            if (Math.Abs(slope) < 0.0001)
            {
                TrendMessage = "Stable readings detected";
                TrendDetail = $"Values are relatively stable with minimal change ({percentChange:F1}% over the period)";
            }
            else if (slope > 0)
            {
                TrendMessage = "Increasing trend detected";
                TrendDetail = $"Values are increasing at approximately {slope:F3} units per hour ({percentChange:F1}% change over the period)";
            }
            else
            {
                TrendMessage = "Decreasing trend detected";
                TrendDetail = $"Values are decreasing at approximately {Math.Abs(slope):F3} units per hour ({percentChange:F1}% change over the period)";
            }
            
            // Add threshold information
            if (CurrentThreshold.HasValue && exceedingThreshold > 0)
            {
                double percentage = 100.0 * exceedingThreshold / n;
                TrendDetail += $"\n\n⚠️ {exceedingThreshold} readings ({percentage:F1}%) exceed safe threshold of {CurrentThreshold:F2} {sortedData[0].Unit}";
            }
            else if (CurrentThreshold.HasValue)
            {
                TrendDetail += $"\n\n✓ All readings are within safe threshold of {CurrentThreshold:F2} {sortedData[0].Unit}";
            }
        }
        
        private string GetAirQualityStatus(string metric, double value)
        {
            switch (metric)
            {
                case "Nitrogen Dioxide":
                    if (value < 40) return "Good";
                    else if (value < 100) return "Moderate";
                    else return "Danger";
                
                case "Sulphur Dioxide":
                    if (value < 50) return "Good";
                    else if (value < 125) return "Moderate";
                    else return "Danger";
                    
                case "PM2.5 Particulate Matter":
                    if (value < 10) return "Good";
                    else if (value < 25) return "Moderate";
                    else return "Danger";
                    
                case "PM10 Particulate Matter":
                    if (value < 20) return "Good";
                    else if (value < 50) return "Moderate";
                    else return "Danger";
                    
                default:
                    return "Normal";
            }
        }
        
        private string GetWaterQualityStatus(string metric, double value)
        {
            switch (metric)
            {
                case "Nitrate":
                    if (value < 25) return "Normal";
                    else if (value < 40) return "Caution";
                    else return "Warning";
                    
                case "Nitrite":
                    if (value < 1) return "Normal";
                    else if (value < 2) return "Caution";
                    else return "Warning";
                    
                case "Phosphate":
                    if (value < 0.05) return "Normal";
                    else if (value < 0.1) return "Caution";
                    else return "Warning";
                    
                case "E. Coli":
                    if (value < 200) return "Normal";
                    else if (value < 400) return "Caution";
                    else return "Warning";
                    
                default:
                    return "Normal";
            }
        }
        
        private string GetWeatherStatus(string metric, double value)
        {
            switch (metric)
            {
                case "Temperature":
                    if (value < 0) return "Cold";
                    else if (value < 20) return "Normal";
                    else return "Hot";
                    
                case "Relative Humidity":
                    if (value < 30) return "Dry";
                    else if (value < 70) return "Normal";
                    else return "Humid";
                    
                case "Wind Speed":
                    if (value < 3) return "Low";
                    else if (value < 8) return "Moderate";
                    else return "High";
                    
                case "Wind Direction":
                    if (value >= 0 && value < 90) return "North-East";
                    else if (value >= 90 && value < 180) return "South-East";
                    else if (value >= 180 && value < 270) return "South-West";
                    else return "North-West";
                    
                default:
                    return "Normal";
            }
        }
        
        private double? GetThresholdForMetric(string metric)
        {
            switch (metric)
            {
                case "Nitrogen Dioxide": return 200.0; // µg/m³
                case "Sulphur Dioxide": return 266.0; // µg/m³
                case "PM2.5 Particulate Matter": return 35.0; // µg/m³
                case "PM10 Particulate Matter": return 50.0; // µg/m³
                
                case "Nitrate": return 50.0; // mg/l
                case "Nitrite": return 3.0; // mg/l
                case "Phosphate": return 0.1; // mg/l
                case "E. Coli": return 500.0; // cfu/100ml
                
                // No specific thresholds for weather data
                default: return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}