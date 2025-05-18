using Microsoft.Data.Sqlite;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SET09102.Administrator.Pages;

/// <summary>
/// Page for configuring sensor settings and parameters in the Administrator section.
/// Allows administrators to view sensors, update their configurations, and perform
/// firmware updates.
/// </summary>
/// <remarks>
/// This page implements INotifyPropertyChanged to support data binding for sensor data.
/// It provides commands for updating sensor configurations and firmware versions.
/// </remarks>
public partial class SensorConfigurationPage : ContentPage, INotifyPropertyChanged
{
    private readonly ISensorService _sensorService;
    private readonly IFirmwareService _firmwareService;
    private ObservableCollection<Sensor> _sensors = [];
    
    /// <summary>
    /// Gets the command for updating sensor firmware.
    /// </summary>
    public ICommand UpdateFirmwareCommand { get; }
    
    /// <summary>
    /// Gets the command for updating sensor configuration.
    /// </summary>
    public ICommand UpdateConfigurationCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensorConfigurationPage"/> class.
    /// </summary>
    /// <param name="sensorService">Service for sensor data access.</param>
    /// <param name="firmwareService">Service for firmware management.</param>
    public SensorConfigurationPage(ISensorService sensorService, IFirmwareService firmwareService)
    {
        // Store service references
        _sensorService = sensorService;
        _firmwareService = firmwareService;
        
        // Initialize commands
        UpdateFirmwareCommand = new Command<int>(OnUpdateFirmware);
        UpdateConfigurationCommand = new Command<int>(OnUpdateConfiguration);
        
        // Initialize UI components
        InitializeComponent();
        
        // Set data binding context
        BindingContext = this;
        
        // Load sensor data
        _ = LoadSensorsAsync();
    }

    /// <summary>
    /// Gets or sets the collection of sensors to display.
    /// </summary>
    public ObservableCollection<Sensor> Sensors
    {
        get => _sensors;
        set
        {
            _sensors = value;
            OnPropertyChanged(nameof(Sensors));
        }
    }

    /// <summary>
    /// Handles the firmware update action for a sensor.
    /// </summary>
    /// <param name="sensorId">The ID of the sensor to update.</param>
    public async void OnUpdateFirmware(int sensorId)
    {
        // Find the sensor in the collection
        var sensor = Sensors.FirstOrDefault(s => s.Id == sensorId);

        if (sensor != null)
        {
            try
            {
                // Get the next version number from the firmware service
                var newVersion = _firmwareService.GetNextVersion(sensor.FirmwareVersion);

                // Confirm the update with the user
                var performUpdate = await DisplayAlert("Update Firmware",
                    $"Are you sure you want to update firmware from '{sensor.FirmwareVersion}' to '{newVersion}' for sensor '{sensor.Name}'?", "Yes", "No");

                if (!performUpdate) return;

                // Create a clone of the sensor to update
                var updatedSensor = sensor.Clone();
                updatedSensor.FirmwareVersion = newVersion;
                
                // Update in the database
                await _sensorService.UpdateSensorAsync(updatedSensor);
                
                // Update the local sensor object
                sensor.FirmwareVersion = newVersion;
                
                // Display confirmation to the user
                await DisplayAlert("Simulating Firmware Update", $"Simulating a firmware update for sensor {sensor.Name} (ID: {sensor.Id}) to '{newVersion}'.", "OK");               
            }
            catch (Exception ex)
            {
                // Display error message
                await DisplayAlert(
                    "Error",
                    $"Error settings firmware version for sensor '{sensor.Id}' - {ex.Message}",
                    "OK");
            }
        }
        else
        {
            // Sensor not found
            await DisplayAlert(
                "Error", $"No sensor with the ID '{sensorId}' was found.", "OK");
        }
    }

    /// <summary>
    /// Handles the configuration update action for a sensor.
    /// </summary>
    /// <param name="sensorId">The ID of the sensor to update.</param>
    public async void OnUpdateConfiguration(int sensorId)
    {
        // Find the sensor in the collection
        var sensor = Sensors.FirstOrDefault(s => s.Id == sensorId);

        if (sensor != null)
        {
            try
            {
                // Prompt the user for a new location value
                var newLocation = await DisplayPromptAsync(
                    "Update Sensor Configuration",
                    "Enter new location (Using 'Location' as an arbitrary field to demonstrate that the sensor's config can be changed):",
                    initialValue: sensor.Location,
                    maxLength: 250                    
                );

                // Return if the user canceled or entered an empty value
                if (string.IsNullOrEmpty(newLocation)) return;

                // Create a clone of the sensor to update
                var updatedSensor = sensor.Clone();
                updatedSensor.Location = newLocation;
                
                // Update in the database
                await _sensorService.UpdateSensorAsync(updatedSensor);
                
                // Update the local sensor object
                sensor.Location = newLocation;
                
                // Display confirmation to the user
                await DisplayAlert("Updated Configuration", $"Updated location configuration for sensor {sensor.Name} (ID: {sensor.Id}) to '{newLocation}'.", "OK");
            }
            catch (Exception ex)
            {
                // Display error message
                await DisplayAlert(
                    "Error",
                    $"Error settings configuration for sensor '{sensor.Id}' - {ex.Message}",
                    "OK");
            }
        }
        else
        {
            // Sensor not found
            await DisplayAlert(
                "Error", $"No sensor with the ID '{sensorId}' was found.", "OK");
        }
    }

    /// <summary>
    /// Loads the list of sensors from the data service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadSensorsAsync()
    {
        // Load sensors from the service and store in the Sensors collection
        Sensors = await _sensorService.GetSensorsAsync();
    }    
}