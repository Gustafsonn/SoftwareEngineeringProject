using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

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

	public EnvTrendPage()
	{
		InitializeComponent();
		LoadMockData();
		DataPointsView.ItemsSource = _dataPoints;
	}

	private void LoadMockData()
	{
		// Mock parameters
		var parameters = new List<string> { "Temperature", "Humidity", "CO2" };
		ParameterPicker.ItemsSource = parameters;

		// Mock locations
		var locations = new List<string> { "Sensor 1", "Sensor 2" };
		LocationPicker.ItemsSource = locations;

		// Set default dates
		StartDatePicker.Date = DateTime.Now.AddMonths(-1);
		EndDatePicker.Date = DateTime.Now;
	}

	private async void OnGenerateReportClicked(object sender, EventArgs e)
	{
		if (ParameterPicker.SelectedItem == null || LocationPicker.SelectedItem == null)
		{
			await DisplayAlert("Error", "Please select both a parameter and location", "OK");
			return;
		}

		// Set report header
		ReportHeader.Text = $"Report for {ParameterPicker.SelectedItem} at {LocationPicker.SelectedItem} from {StartDatePicker.Date:d} to {EndDatePicker.Date:d}";

		// Mock data points
		_dataPoints.Clear();
		var random = new Random();
		for (int i = 0; i < 7; i++)
		{
			_dataPoints.Add(new DataPoint
			{
				Date = DateTime.Now.AddDays(-6 + i).ToString("dd/MM/yyyy"),
				Value = Math.Round(15 + random.NextDouble() * 10, 2),
				Status = "OK"
			});
		}

		// Calculate statistics
		if (_dataPoints.Count > 0)
		{
			double avg = 0, max = double.MinValue, min = double.MaxValue;
			foreach (var dp in _dataPoints)
			{
				avg += dp.Value;
				if (dp.Value > max) max = dp.Value;
				if (dp.Value < min) min = dp.Value;
			}
			avg /= _dataPoints.Count;
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