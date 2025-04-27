using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    /// <summary>
    /// Dialog for configuring sensor-specific settings.
    /// </summary>
    /// <remarks>
    /// This page provides a user interface for viewing and modifying sensor configuration settings.
    /// </remarks>
    public partial class SensorSettingsDialog : ContentPage, INotifyPropertyChanged
    {
        private readonly SensorSettingsService _settingsService;
        private readonly Sensor _sensor;
        private SensorSettings _settings;
        private string _thresholdDisplayValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorSettingsDialog"/> class.
        /// </summary>
        /// <param name="databaseService">The database service to use for data operations.</param>
        /// <param name="sensor">The sensor to configure.</param>
        /// <remarks>
        /// Initializes the dialog with default settings and then loads the
        /// sensor-specific settings from the database.
        /// </remarks>
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

        /// <summary>
        /// Gets the sensor being configured.
        /// </summary>
        /// <value>
        /// The sensor associated with this dialog.
        /// </value>
        public Sensor Sensor => _sensor;
        
        /// <summary>
        /// Gets the name of the sensor being configured.
        /// </summary>
        /// <value>
        /// The name of the sensor, or "Unnamed Sensor" if the sensor is null.
        /// </value>
        public string SensorName => _sensor?.Name ?? "Unnamed Sensor";
        
        /// <summary>
        /// Gets or sets the settings for the sensor.
        /// </summary>
        /// <value>
        /// The sensor settings.
        /// </value>
        /// <remarks>
        /// When this property changes, <see cref="UpdateThresholdDisplay"/> is called
        /// to update the threshold display value. Changes to this property trigger
        /// the PropertyChanged event.
        /// </remarks>
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
        
        /// <summary>
        /// Gets or sets the formatted display value for the alert threshold.
        /// </summary>
        /// <value>
        /// A string representation of the alert threshold.
        /// </value>
        /// <remarks>
        /// This property is updated by <see cref="UpdateThresholdDisplay"/> and
        /// <see cref="OnAlertThresholdChanged"/>. Changes to this property trigger
        /// the PropertyChanged event.
        /// </remarks>
        public string ThresholdDisplayValue
        {
            get => _thresholdDisplayValue;
            set
            {
                _thresholdDisplayValue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Handles changes to the alert threshold slider.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data containing the new slider value.</param>
        /// <remarks>
        /// Updates the threshold display value when the slider value changes.
        /// </remarks>
        private void OnAlertThresholdChanged(object sender, ValueChangedEventArgs e)
        {
            UpdateThresholdDisplay();
        }

        /// <summary>
        /// Handles the test connection button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        /// <remarks>
        /// Simulates a connection test to the sensor. In a real implementation,
        /// this would attempt to establish a connection to the physical sensor.
        /// </remarks>
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

        /// <summary>
        /// Handles the calibrate now button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        /// <remarks>
        /// Simulates a calibration process for the sensor. In a real implementation,
        /// this would trigger a calibration cycle on the physical sensor.
        /// </remarks>
        private async void OnCalibrateNowClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Calibrate Sensor", 
                $"Are you sure you want to calibrate {SensorName} now? This may temporarily interrupt data collection.", 
                "Yes", "No");
                
            if (!confirm)
                return;
                
            // Simulate calibration process
            await DisplayAlert("Calibration", "Calibration process initiated.", "OK");
            
            // Update last calibration date
            _sensor.LastCalibration = DateTime.Now;
            _sensor.NextCalibration = DateTime.Now.AddDays(Settings.CalibrationIntervalDays);
        }

        /// <summary>
        /// Handles the reset to defaults button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        /// <remarks>
        /// Resets all sensor settings to their default values after confirmation.
        /// </remarks>
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

        /// <summary>
        /// Handles the cancel button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        /// <remarks>
        /// Closes the dialog without saving any changes.
        /// </remarks>
        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        /// <summary>
        /// Handles the save changes button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        /// <remarks>
        /// Saves the current settings to the database and closes the dialog.
        /// </remarks>
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

        /// <summary>
        /// Loads sensor settings from the database.
        /// </summary>
        /// <remarks>
        /// This method is called during initialization to load the
        /// sensor-specific settings from the database.
        /// </remarks>
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

        /// <summary>
        /// Updates the threshold display value based on current settings.
        /// </summary>
        /// <remarks>
        /// Calculates and formats the threshold value by multiplying the sensor's
        /// maximum threshold by the alert threshold percentage.
        /// </remarks>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// This event is used by the XAML binding engine to detect changes to properties.
        /// </remarks>
        public new event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}