using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace SET09102.EnvironmentalScientist.Pages;

public partial class EnvTrendPage : ContentPage
{
	public EnvTrendPage()
	{
		InitializeComponent();
		LoadMockData();
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
		await DisplayAlert("Report", $"Generating report for {ParameterPicker.SelectedItem} at {LocationPicker.SelectedItem} from {StartDatePicker.Date:d} to {EndDatePicker.Date:d}", "OK");
	}
}