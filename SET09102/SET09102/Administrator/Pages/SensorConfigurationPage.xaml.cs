using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SET09102.Administrator.Pages;

public partial class SensorConfigurationPage : ContentPage, INotifyPropertyChanged
{
    private readonly ISensorService _sensorService;
    private ObservableCollection<Sensor> _sensors = [];
    public Command<int> UpdateFirmwareCommand { get; }
    public Command<int> UpdateConfigurationCommand { get; }

    public SensorConfigurationPage(ISensorService sensorService)
    {
        _sensorService = sensorService;
        InitializeComponent();
        BindingContext = this;
        UpdateFirmwareCommand = new Command<int>(OnUpdateFirmware);
        UpdateConfigurationCommand = new Command<int>(OnUpdateConfiguration);
        _ = LoadSensorsAsync();
    }

    public ObservableCollection<Sensor> Sensors
    {
        get => _sensors;
        set
        {
            _sensors = value;
            OnPropertyChanged(nameof(Sensors));
        }
    }

    public async void OnUpdateFirmware(int sensorId)
    {
        var sensor = Sensors.FirstOrDefault(s => s.Id == sensorId);

        if (sensor != null)
        {
            try
            {
                var newVersion = GetNextFirmwareVersion(sensor.FirmwareVersion);

                var performUpdate = await DisplayAlert("Update Firmware",
                    $"Are you sure you want to update firmware from '{sensor.FirmwareVersion}' to '{newVersion}' for sensor '{sensor.Name}'?", "Yes", "No");

                if (!performUpdate) return;

                var updatedSensor = sensor.Clone();
                updatedSensor.FirmwareVersion = newVersion;
                await _sensorService.UpdateSensor(updatedSensor);
                sensor.FirmwareVersion = newVersion;
                await DisplayAlert("Simulating Firmware Update", $"Simulating a firmware update for sensor {sensor.Name} (ID: {sensor.Id}) to '{newVersion}'.", "OK");               
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Error settings firmware version for sensor '{sensor.Id}' - {ex.Message}",
                    "OK");
            }
        }
        else
        {
            await DisplayAlert(
                "Error", $"No sensor with the ID '{sensorId}' was found.", "OK");
        }
    }

    public async void OnUpdateConfiguration(int sensorId)
    {
        var sensor = Sensors.FirstOrDefault(s => s.Id == sensorId);

        if (sensor != null)
        {
            try
            {
                var newLocation = await DisplayPromptAsync(
                    "Update Sensor Configuration",
                    "Enter new location (Using 'Location' as an arbitrary field to demonstrate that the sensor's config can be changed):",
                    initialValue: sensor.Location,
                    maxLength: 250                    
                );

                if (string.IsNullOrEmpty(newLocation)) return;

                var updatedSensor = sensor.Clone();
                updatedSensor.Location = newLocation;
                await _sensorService.UpdateSensor(updatedSensor);
                sensor.Location = newLocation;
                await DisplayAlert("Updated Configuration", $"Updated location configuration for sensor {sensor.Name} (ID: {sensor.Id}) to '{newLocation}'.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    "Error",
                    $"Error settings configuration for sensor '{sensor.Id}' - {ex.Message}",
                    "OK");
            }
        }
        else
        {
            await DisplayAlert(
                "Error", $"No sensor with the ID '{sensorId}' was found.", "OK");
        }
    }

    private async Task LoadSensorsAsync()
    {
        Sensors = await _sensorService.GetSensorsAsync();
    }

    private static string GetNextFirmwareVersion(string firmwareVersion)
    {
        var versionParts = firmwareVersion.Split('.');

        if (versionParts.Length == 3)
        {
            if (int.TryParse(versionParts[0], out int major) &&
                int.TryParse(versionParts[1], out int minor) &&
                int.TryParse(versionParts[2], out int patch))
            {
                major++;
                return $"{major}.0.0";
            }
        }

        throw new Exception($"The firmware version was not in the expected format - {firmwareVersion}");
    }
}