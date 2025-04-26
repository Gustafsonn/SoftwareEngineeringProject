using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;

namespace SET09102.Administrator.Pages
{
    public partial class SensorConfigurationPage : ContentPage
    {
        private readonly ISensorConfigurationService _sensorService;
        private Sensor _selectedSensor;

        public SensorConfigurationPage(ISensorConfigurationService sensorService)
        {
            InitializeComponent();
            _sensorService = sensorService;
            LoadSensors();
        }

        private async void LoadSensors()
        {
            try
            {
                var sensors = await _sensorService.GetAllSensorsAsync();
                SensorPicker.ItemsSource = sensors;
                SensorPicker.ItemDisplayBinding = new Binding("Name");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
        }

        private async void OnSensorSelected(object sender, EventArgs e)
        {
            if (SensorPicker.SelectedItem is Sensor sensor)
            {
                _selectedSensor = sensor;
                await LoadSensorConfiguration();
            }
        }

        private async Task LoadSensorConfiguration()
        {
            try
            {
                var config = await _sensorService.GetSensorConfigurationAsync(_selectedSensor.Id);
                CurrentFirmwareLabel.Text = $"Firmware Version: {config.FirmwareVersion}";
                CurrentConfigLabel.Text = $"Configuration: Polling Interval - {config.PollingInterval}s, Threshold - {config.AlertThreshold}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load configuration: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateConfigClicked(object sender, EventArgs e)
        {
            if (_selectedSensor == null)
            {
                await DisplayAlert("Error", "Please select a sensor first", "OK");
                return;
            }

            try
            {
                if (!int.TryParse(PollingIntervalEntry.Text, out int pollingInterval) ||
                    !double.TryParse(ThresholdEntry.Text, out double threshold))
                {
                    await DisplayAlert("Error", "Please enter valid numbers", "OK");
                    return;
                }

                var config = new SensorConfiguration
                {
                    PollingInterval = pollingInterval,
                    AlertThreshold = threshold
                };

                await _sensorService.UpdateSensorConfigurationAsync(_selectedSensor.Id, config);
                await DisplayAlert("Success", "Configuration updated successfully", "OK");
                await LoadSensorConfiguration();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update configuration: {ex.Message}", "OK");
            }
        }

        private async void OnCheckUpdatesClicked(object sender, EventArgs e)
        {
            if (_selectedSensor == null)
            {
                await DisplayAlert("Error", "Please select a sensor first", "OK");
                return;
            }

            try
            {
                var updateAvailable = await _sensorService.CheckFirmwareUpdateAsync(_selectedSensor.Id);
                UpdateFirmwareButton.IsEnabled = updateAvailable;
                UpdateStatusLabel.Text = updateAvailable ? "Update available!" : "Sensor is up to date";
                UpdateStatusLabel.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to check for updates: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateFirmwareClicked(object sender, EventArgs e)
        {
            if (_selectedSensor == null)
            {
                await DisplayAlert("Error", "Please select a sensor first", "OK");
                return;
            }

            try
            {
                UpdateProgress.IsVisible = true;
                UpdateStatusLabel.Text = "Updating firmware...";
                UpdateFirmwareButton.IsEnabled = false;

                await _sensorService.UpdateFirmwareAsync(_selectedSensor.Id, progress =>
                {
                    UpdateProgress.Progress = progress;
                });

                UpdateStatusLabel.Text = "Firmware updated successfully!";
                await LoadSensorConfiguration();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update firmware: {ex.Message}", "OK");
                UpdateStatusLabel.Text = "Update failed";
            }
            finally
            {
                UpdateProgress.IsVisible = false;
                UpdateFirmwareButton.IsEnabled = true;
            }
        }
    }
} 