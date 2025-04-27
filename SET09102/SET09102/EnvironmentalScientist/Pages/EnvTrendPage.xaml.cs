using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using SET09102.Services;
using SET09102.Models;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SET09102.EnvironmentalScientist.Pages;

public class DataPoint
{
	public string Date { get; set; } = string.Empty;
	public double Value { get; set; }
	public string Status { get; set; } = string.Empty;
}

public partial class EnvTrendPage : ContentPage
{
	private ObservableCollection<DataPoint> _dataPoints = new();
	private readonly DatabaseService _databaseService = new DatabaseService();
	private ObservableCollection<Sensor> _allSensors = new();

	public EnvTrendPage()
	{
		InitializeComponent();
		_ = LoadDataAsync();
		DataPointsView.ItemsSource = _dataPoints;
		ParameterPicker.SelectedIndexChanged += OnParameterChanged;
	}

	private async Task LoadDataAsync()
	{
		// Parameters (should match DataType in your tables)
		var parameters = new List<string> { "Air Quality", "Water Quality", "Weather" };
		ParameterPicker.ItemsSource = parameters;

		// Load all sensors from the database
		var sensorService = new SensorService(_databaseService.GetDatabasePath());
		var sensors = await sensorService.GetSensorsAsync();
		_allSensors = sensors;

		// Set default dates
		StartDatePicker.Date = DateTime.Now.AddMonths(-1);
		EndDatePicker.Date = DateTime.Now;

		// Set initial sensor filter
		FilterSensorsByParameter();
	}

	private void OnParameterChanged(object sender, EventArgs e)
	{
		FilterSensorsByParameter();
	}

	private void FilterSensorsByParameter()
	{
		if (ParameterPicker.SelectedItem == null) return;
		string selectedType = ParameterPicker.SelectedItem.ToString() ?? "";
		var filtered = _allSensors.Where(s => s.Type == selectedType).ToList();
		LocationPicker.ItemsSource = filtered;
		LocationPicker.ItemDisplayBinding = new Binding("Name");
		if (filtered.Count > 0)
			LocationPicker.SelectedIndex = 0;
	}

	private async void OnGenerateReportClicked(object sender, EventArgs e)
	{
		if (ParameterPicker.SelectedItem == null || LocationPicker.SelectedItem == null)
		{
			await DisplayAlert("Error", "Please select both a parameter and location", "OK");
			return;
		}

		var selectedSensor = LocationPicker.SelectedItem as Sensor;
		string sensorName = selectedSensor?.Name ?? "Unknown Sensor";
		string selectedType = ParameterPicker.SelectedItem?.ToString() ?? "";
		DateTime start = StartDatePicker.Date;
		DateTime end = EndDatePicker.Date;

		// Set report header
		ReportHeader.Text = $"Report for {selectedType} at {sensorName} from {start:d} to {end:d}";

		_dataPoints.Clear();
		List<DataPoint> points = new();

		using (var connection = new SqliteConnection($"Data Source={_databaseService.GetDatabasePath()}"))
		{
			await connection.OpenAsync();
			SqliteCommand command = connection.CreateCommand();

			if (selectedType == "Air Quality")
			{
				// Map sensor name to column
				string col = sensorName switch
				{
					"Nitrogen Dioxide Sensor" => "nitrogen_dioxide",
					"Sulphur Dioxide Sensor" => "sulphur_dioxide",
					"PM2.5 Sensor" => "pm25_particulate_matter",
					"PM10 Sensor" => "pm10_particulate_matter",
					_ => null
				};
				if (col != null)
				{
					command.CommandText = $@"SELECT date, time, {col} FROM air_quality WHERE {col} IS NOT NULL AND date >= @start AND date <= @end ORDER BY date, time";
					command.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@end", end.ToString("yyyy-MM-dd"));
					using var reader = await command.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						string date = reader.GetString(0);
						string time = reader.GetString(1);
						double value = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
						points.Add(new DataPoint { Date = $"{date} {time}", Value = value, Status = "OK" });
					}
				}
			}
			else if (selectedType == "Water Quality")
			{
				string col = sensorName switch
				{
					"Nitrate Sensor" => "nitrate",
					"Nitrite Sensor" => "nitrite",
					"Phosphate Sensor" => "phosphate",
					"E. Coli Sensor" => "ec_cfu_per_100ml",
					_ => null
				};
				if (col != null)
				{
					command.CommandText = $@"SELECT date, time, {col} FROM water_quality WHERE {col} IS NOT NULL AND date >= @start AND date <= @end ORDER BY date, time";
					command.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@end", end.ToString("yyyy-MM-dd"));
					using var reader = await command.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						string date = reader.GetString(0);
						string time = reader.GetString(1);
						double value = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
						points.Add(new DataPoint { Date = $"{date} {time}", Value = value, Status = "OK" });
					}
				}
			}
			else if (selectedType == "Weather")
			{
				string col = sensorName switch
				{
					"Temperature Sensor" => "temperature_2m",
					"Humidity Sensor" => "relative_humidity_2m",
					"Wind Speed Sensor" => "wind_speed_10m",
					"Wind Direction Sensor" => "wind_direction_10m",
					_ => null
				};
				if (col != null)
				{
					command.CommandText = $@"SELECT time, {col} FROM weather_conditions WHERE {col} IS NOT NULL AND time >= @start AND time <= @end ORDER BY time";
					command.Parameters.AddWithValue("@start", start.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@end", end.ToString("yyyy-MM-dd"));
					using var reader = await command.ExecuteReaderAsync();
					while (await reader.ReadAsync())
					{
						string time = reader.GetString(0);
						double value = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
						points.Add(new DataPoint { Date = time, Value = value, Status = "OK" });
					}
				}
			}
		}

		foreach (var dp in points)
			_dataPoints.Add(dp);

		// Calculate statistics
		if (_dataPoints.Count > 0)
		{
			double avg = _dataPoints.Average(dp => dp.Value);
			double max = _dataPoints.Max(dp => dp.Value);
			double min = _dataPoints.Min(dp => dp.Value);
			AverageValue.Text = avg.ToString("F2");
			MaxValue.Text = max.ToString("F2");
			MinValue.Text = min.ToString("F2");
		}
		else
		{
			AverageValue.Text = MaxValue.Text = MinValue.Text = "N/A";
		}
	}
}