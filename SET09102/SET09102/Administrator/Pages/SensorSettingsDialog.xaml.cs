using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    public partial class SensorSettingsDialog : ContentPage, INotifyPropertyChanged
    {
        private readonly SensorSettingsService _settingsService;
        private readonly Sensor _sensor;
        private SensorSettings _settings;
        private string _thresholdDisplayValue;

        public SensorSettingsDialog(DatabaseService databaseService, Sensor sensor)
        {
            InitializeComponent();
            
            _settingsService = new SensorSettingsService(databaseService);
            _sensor = sensor;
            _settings = new SensorSettings(); // Default settings
            
            // Set binding context
            BindingContext = this;
            
            // Load settings
            LoadSettingsAsync();
        }

        // Properties
        public Sensor Sensor => _sensor;
        
        public string SensorName => _sensor?.Name ?? "Unnamed Sensor";
        
        public SensorSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
                UpdateThresholdDisplay();
            }
        }
        
        public string ThresholdDisplayValue
        {
            get => _thresholdDisplayValue;
            set
            {
                _thresholdDisplayValue = value;
                OnPropertyChanged();
            }
        }

        // Event handlers
        private void OnAlertThresholdChanged(object sender, ValueChangedEventArgs e)
        {
            UpdateThresholdDisplay();
        }

        private async void OnTestConnectionClicked(object sender, EventArgs e)
        {
            // Simulate a connection test
            await DisplayAlert("Testing Connection", $"Testing connection to {SensorName}...", "Cancel");
            
            // Random success/failure for demo
            bool success = new Random().Next(100) < 90; // 90% success rate
            
            if (success)
            {
                await DisplayAlert("Connection Successful", $"Successfully connected to {SensorName}.", "OK");
            }
            else
            {
                await DisplayAlert("Connection Failed", $"Failed to connect to {SensorName}. Please check your network settings and try again.", "OK");
            }
        }

        private async void OnCalibrateNowClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Calibrate Sensor", 
                $"Are you sure you want to calibrate {SensorName} now? This may temporarily interrupt data collection.", 
                "Yes", "No");
                
            if (!confirm)
                return;
                
            // Simulate calibration process
            await DisplayAlert("Calibration", "Calibration process initiated. This would trigger a calibration cycle on the device in a real implementation.", "OK");
            
            // Update last calibration date
            _sensor.LastCalibration = DateTime.Now;
            _sensor.NextCalibration = DateTime.Now.AddDays(Settings.CalibrationIntervalDays);
        }

        private async void OnResetToDefaultsClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Reset Settings", 
                "Are you sure you want to reset all settings to defaults? This cannot be undone.", 
                "Reset", "Cancel");
                
            if (!confirm)
                return;
                
            // Reset to defaults
            Settings = new SensorSettings();
            
            // Update UI
            IntervalPicker.SelectedItem = Settings.DataCollectionInterval;
            FirmwarePicker.SelectedItem = Settings.FirmwareUpdatePolicy;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnSaveChangesClicked(object sender, EventArgs e)
        {
            try
            {
                // Save settings to database
                bool success = await _settingsService.SaveSensorSettingsAsync(_sensor.Id, Settings);
                
                if (success)
                {
                    await DisplayAlert("Success", "Sensor settings saved successfully.", "OK");
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to save sensor settings. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Helper methods
        private async void LoadSettingsAsync()
        {
            try
            {
                // Load settings from database
                Settings = await _settingsService.GetSensorSettingsAsync(_sensor.Id);
                
                // Update UI
                IntervalPicker.SelectedItem = Settings.DataCollectionInterval;
                FirmwarePicker.SelectedItem = Settings.FirmwareUpdatePolicy;
                UpdateThresholdDisplay();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensor settings: {ex.Message}", "OK");
            }
        }

        private void UpdateThresholdDisplay()
        {
            if (_sensor != null && _sensor.MaxThreshold.HasValue && Settings != null)
            {
                double thresholdValue = _sensor.MaxThreshold.Value * Settings.AlertThreshold;
                int percent = (int)(Settings.AlertThreshold * 100);
                ThresholdDisplayValue = $"{thresholdValue:F2} {_sensor.Unit} ({percent}% of max)";
            }
            else
            {
                ThresholdDisplayValue = "N/A";
            }
        }

        // INotifyPropertyChanged implementation
        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}