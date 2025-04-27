using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SET09102.Models;
using SET09102.Services;

namespace SET09102.Administrator.ViewModels
{
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

        // Properties
        public ObservableCollection<Sensor> Sensors
        {
            get => _filteredSensors;
            set
            {
                _filteredSensors = value;
                OnPropertyChanged();
            }
        }

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

        public bool HasSelectedSensor
        {
            get => _hasSelectedSensor;
            set
            {
                _hasSelectedSensor = value;
                OnPropertyChanged();
            }
        }

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

        public string DataCollectionInterval
        {
            get => _dataCollectionInterval;
            set
            {
                _dataCollectionInterval = value;
                OnPropertyChanged();
            }
        }

        public bool AutoCalibration
        {
            get => _autoCalibration;
            set
            {
                _autoCalibration = value;
                OnPropertyChanged();
            }
        }

        public bool AlertNotifications
        {
            get => _alertNotifications;
            set
            {
                _alertNotifications = value;
                OnPropertyChanged();
            }
        }

        // Methods
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}