using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SET09102.Models;
using SET09102.Services;

namespace SET09102.Administrator.ViewModels
{
    /// <summary>
    /// View model for managing sensor configurations and settings in the administrator interface.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for filtering, selecting, and modifying sensor data.
    /// </remarks>
    public class SensorManagementViewModel : INotifyPropertyChanged
    {
        private readonly SensorService _sensorService;
        private ObservableCollection<Sensor> _allSensors;
        private ObservableCollection<Sensor> _filteredSensors;
        private Sensor _selectedSensor;
        private bool _hasSelectedSensor;
        private string _selectedSensorType;
        
        // Settings properties
        private string _dataCollectionInterval;
        private bool _autoCalibration;
        private bool _alertNotifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorManagementViewModel"/> class.
        /// </summary>
        /// <param name="dbPath">The path to the database file.</param>
        public SensorManagementViewModel(string dbPath)
        {
            _sensorService = new SensorService(dbPath);
            _allSensors = new ObservableCollection<Sensor>();
            _filteredSensors = new ObservableCollection<Sensor>();
            _selectedSensorType = "All Types";
            _dataCollectionInterval = "1 hour";
            _autoCalibration = true;
            _alertNotifications = true;
        }

        /// <summary>
        /// Gets or sets the collection of sensors currently being displayed.
        /// </summary>
        /// <value>
        /// The filtered collection of sensors.
        /// </value>
        /// <remarks>
        /// This property is filtered based on the <see cref="SelectedSensorType"/> property.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public ObservableCollection<Sensor> Sensors
        {
            get => _filteredSensors;
            set
            {
                _filteredSensors = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected sensor.
        /// </summary>
        /// <value>
        /// The selected sensor, or null if no sensor is selected.
        /// </value>
        /// <remarks>
        /// When this property changes, <see cref="HasSelectedSensor"/> is automatically updated.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public Sensor SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                _selectedSensor = value;
                HasSelectedSensor = (_selectedSensor != null);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a sensor is currently selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if a sensor is selected; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is automatically updated when <see cref="SelectedSensor"/> changes.
        /// It is used to enable or disable UI elements that operate on the selected sensor.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool HasSelectedSensor
        {
            get => _hasSelectedSensor;
            set
            {
                _hasSelectedSensor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the type of sensor to filter by.
        /// </summary>
        /// <value>
        /// The selected sensor type (e.g "Air Quality", "Water Quality"), or "All Types" to show all sensors.
        /// </value>
        /// <remarks>
        /// When this property changes, the <see cref="Sensors"/> collection is automatically filtered.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public string SelectedSensorType
        {
            get => _selectedSensorType;
            set
            {
                _selectedSensorType = value;
                OnPropertyChanged();
                FilterSensors();
            }
        }

        /// <summary>
        /// Gets or sets the data collection interval.
        /// </summary>
        /// <value>
        /// A string representation of the data collection interval (e.g., "1 minute", "1 hour").
        /// </value>
        /// <remarks>
        /// This property is used in global settings that apply to all sensors.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public string DataCollectionInterval
        {
            get => _dataCollectionInterval;
            set
            {
                _dataCollectionInterval = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-calibration is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto-calibration is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is used in global settings that apply to all sensors.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool AutoCalibration
        {
            get => _autoCalibration;
            set
            {
                _autoCalibration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether alert notifications are enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if alert notifications are enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is used in global settings that apply to all sensors.
        /// Changes to this property trigger the PropertyChanged event.
        /// </remarks>
        public bool AlertNotifications
        {
            get => _alertNotifications;
            set
            {
                _alertNotifications = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads all sensors from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error loading sensors from the database.</exception>
        /// <remarks>
        /// After loading the sensors, the <see cref="FilterSensors"/> method is called
        /// to update the <see cref="Sensors"/> collection based on the current filter.
        /// </remarks>
        public async Task LoadSensorsAsync()
        {
            try
            {
                _allSensors = await _sensorService.GetSensorsAsync();
                FilterSensors();
            }
            catch (Exception ex)
            {
                // Handle or propagate the exception
                System.Diagnostics.Debug.WriteLine($"Error loading sensors: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Filters the sensors based on the <see cref="SelectedSensorType"/> property.
        /// </summary>
        /// <remarks>
        /// If <see cref="SelectedSensorType"/> is "All Types" or empty, all sensors are included.
        /// Otherwise, only sensors of the specified type are included.
        /// </remarks>
        public void FilterSensors()
        {
            if (_allSensors == null)
                return;

            if (string.IsNullOrEmpty(_selectedSensorType) || _selectedSensorType == "All Types")
            {
                Sensors = new ObservableCollection<Sensor>(_allSensors);
            }
            else
            {
                Sensors = new ObservableCollection<Sensor>(
                    _allSensors.Where(s => s.Type == _selectedSensorType)
                );
            }
        }

        /// <summary>
        /// Updates a sensor in the database.
        /// </summary>
        /// <param name="sensor">The sensor to update.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
        /// <remarks>
        /// In a real implementation, this would update the sensor in the database.
        /// Currently, this is a placeholder method.
        /// </remarks>
        public async Task<bool> UpdateSensorAsync(Sensor sensor)
        {
            try
            {
                // In a real implementation, this would update the sensor in the database
                // For now, just a placeholder
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating sensor: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the status of a sensor.
        /// </summary>
        /// <param name="sensor">The sensor to update.</param>
        /// <param name="newStatus">The new status to set.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
        /// <remarks>
        /// This method updates the sensor status in memory, but in a real implementation,
        /// it would also update the database.
        /// </remarks>
        public async Task<bool> UpdateSensorStatusAsync(Sensor sensor, string newStatus)
        {
            try
            {
                // In a real implementation, this would update the sensor status in the database
                sensor.Status = newStatus;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating sensor status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the firmware version of a sensor.
        /// </summary>
        /// <param name="sensor">The sensor to update.</param>
        /// <param name="newVersion">The new firmware version.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
        /// <remarks>
        /// This method updates the firmware version in memory, but in a real implementation,
        /// it would also trigger a firmware update process and update the database.
        /// </remarks>
        public async Task<bool> UpdateFirmwareAsync(Sensor sensor, string newVersion)
        {
            try
            {
                // In a real implementation, this would trigger a firmware update
                sensor.FirmwareVersion = newVersion;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating firmware: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves the global settings.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
        /// <remarks>
        /// In a real implementation, this would save the settings to a persistent store.
        /// Currently, this is a placeholder method.
        /// </remarks>
        public async Task<bool> SaveSettingsAsync()
        {
            try
            {
                // In a real implementation, this would save settings to a persistent store
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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