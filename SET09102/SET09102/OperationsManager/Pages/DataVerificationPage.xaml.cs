using Microsoft.Maui.Controls;
using SET09102.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SET09102.OperationsManager.Pages;

public partial class DataVerificationPage : ContentPage
{
	private readonly DatabaseService _databaseService = new();
	private string _selectedTable = "air_quality";

	public DataVerificationPage()
	{
		InitializeComponent();
		// Default to air_quality on load
		RunVerification();
	}

	private void OnShowAirQualityClicked(object sender, EventArgs e)
	{
		_selectedTable = "air_quality";
		RunVerification();
	}

	private void OnShowWaterQualityClicked(object sender, EventArgs e)
	{
		_selectedTable = "water_quality";
		RunVerification();
	}

	private void OnShowWeatherConditionsClicked(object sender, EventArgs e)
	{
		_selectedTable = "weather_conditions";
		RunVerification();
	}

	private void OnRefreshClicked(object sender, EventArgs e)
	{
		RunVerification();
	}

	private async void RunVerification()
	{
		if (string.IsNullOrEmpty(_selectedTable))
		{
			await DisplayAlert("Error", "No data table selected.", "OK");
			return;
		}
		try
		{
			var results = await VerifyTableAsync(_selectedTable);
			DataList.ItemsSource = results;
			int valid = results.Count(r => r.Status == "Valid");
			int invalid = results.Count(r => r.Status == "Invalid");
			int warning = results.Count(r => r.Status == "Warning");
			SummaryLabel.Text = $"Summary: {valid} valid, {invalid} invalid, {warning} warnings";
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to verify data: {ex.Message}", "OK");
		}
	}

	private async Task<List<VerificationResult>> VerifyTableAsync(string table)
	{
		var results = new List<VerificationResult>();
		var dbPath = _databaseService.GetDatabasePath();
		if (!File.Exists(dbPath))
			return results;
		using var connection = _databaseService.GetConnection();
		if (connection.State != System.Data.ConnectionState.Open)
			connection.Open();
		// Check if the table exists
		using (var checkCmd = connection.CreateCommand())
		{
			checkCmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'";
			var exists = await checkCmd.ExecuteScalarAsync();
			if (exists == null)
			{
				await MainThread.InvokeOnMainThreadAsync(async () =>
				{
					await DisplayAlert("Table Not Found", $"The table '{table}' does not exist in the database.", "OK");
				});
				return results;
			}
		}
		using var command = connection.CreateCommand();
		command.CommandText = $"SELECT * FROM {table}";
		using var reader = await command.ExecuteReaderAsync();
		int rowNum = 1;
		while (await reader.ReadAsync())
		{
			if (reader.FieldCount == 0) continue;
			var row = new object[reader.FieldCount];
			reader.GetValues(row);
			var result = CheckRow(table, row, reader);
			result.Display = $"Row {rowNum}";
			result.Details = string.Join(", ", Enumerable.Range(0, reader.FieldCount)
				.Where(i => reader.GetName(i) != null)
				.Select(i => $"{reader.GetName(i)}: {(row[i] == null || row[i] == DBNull.Value ? "(null)" : row[i])}"));
			results.Add(result);
			rowNum++;
		}
		return results;
	}

	private VerificationResult CheckRow(string table, object[] row, IDataRecord reader)
	{
		bool valid = true;
		string status = "Valid";
		string details = "";
		if (table == "air_quality")
		{
			// Columns: id, date, time, nitrogen_dioxide, sulphur_dioxide, pm25_particulate_matter, pm10_particulate_matter
			if (row[1] == DBNull.Value || string.IsNullOrWhiteSpace(row[1]?.ToString())) { valid = false; details += "Date missing. "; }
			if (row[2] == DBNull.Value || string.IsNullOrWhiteSpace(row[2]?.ToString())) { valid = false; details += "Time missing. "; }
			if (row[3] == DBNull.Value || !IsNonNegative(row[3])) { valid = false; details += "NO2 invalid. "; }
			if (row[4] == DBNull.Value || !IsNonNegative(row[4])) { valid = false; details += "SO2 invalid. "; }
			if (row[5] == DBNull.Value || !IsNonNegative(row[5])) { valid = false; details += "PM2.5 invalid. "; }
			if (row[6] == DBNull.Value || !IsNonNegative(row[6])) { valid = false; details += "PM10 invalid. "; }
		}
		else if (table == "water_quality")
		{
			// Columns: id, date, time, nitrate, nitrite, phosphate, ec_cfu_per_100ml
			if (row[1] == DBNull.Value || string.IsNullOrWhiteSpace(row[1]?.ToString())) { valid = false; details += "Date missing. "; }
			if (row[2] == DBNull.Value || string.IsNullOrWhiteSpace(row[2]?.ToString())) { valid = false; details += "Time missing. "; }
			if (row[3] == DBNull.Value || !IsNonNegative(row[3])) { valid = false; details += "Nitrate invalid. "; }
			if (row[4] == DBNull.Value || !IsNonNegative(row[4])) { valid = false; details += "Nitrite invalid. "; }
			if (row[5] == DBNull.Value || !IsNonNegative(row[5])) { valid = false; details += "Phosphate invalid. "; }
			if (row[6] == DBNull.Value || !IsNonNegative(row[6])) { valid = false; details += "EC invalid. "; }
		}
		else if (table == "weather_conditions")
		{
			// Columns: id, time, temperature_2m, relative_humidity_2m, wind_speed_10m, wind_direction_10m
			if (row[1] == DBNull.Value || string.IsNullOrWhiteSpace(row[1]?.ToString())) { valid = false; details += "Time missing. "; }
			if (row[2] == DBNull.Value || !IsNumeric(row[2])) { valid = false; details += "Temp invalid. "; }
			if (row[3] == DBNull.Value || !IsNumeric(row[3])) { valid = false; details += "Humidity invalid. "; }
			if (row[4] == DBNull.Value || !IsNumeric(row[4])) { valid = false; details += "Wind speed invalid. "; }
			if (row[5] == DBNull.Value || !IsNumeric(row[5])) { valid = false; details += "Wind dir invalid. "; }
		}
		if (!valid) status = "Invalid";
		return new VerificationResult { Status = status, Details = details };
	}

	private bool IsNonNegative(object value)
	{
		if (value == null || value == DBNull.Value) return false;
		if (double.TryParse(value.ToString(), out double d)) return d >= 0;
		return false;
	}
	private bool IsNumeric(object value)
	{
		if (value == null || value == DBNull.Value) return false;
		return double.TryParse(value.ToString(), out _);
	}

	private class VerificationResult
	{
		public string Display { get; set; }
		public string Status { get; set; }
		public string Details { get; set; }
	}
}