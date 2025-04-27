using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    public partial class SettingsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly SensorService _sensorService;
        private ObservableCollection<Sensor> _allSensors;
        private ObservableCollection<Sensor> _filteredSensors;
        private Sensor _selectedSensor;
        private bool _hasSelectedSensor;
        
        private string _dataCollectionInterval = "1 hour";
        private bool _autoCalibration = true;
        private bool _alertNotifications = true;

        public SettingsPage()
        {
            InitializeComponent();
            
            string dbPath = new DatabaseService().GetDatabasePath();
            _sensorService = new SensorService(dbPath);
            
            _allSensors = new ObservableCollection<Sensor>();
            _filteredSensors = new ObservableCollection<Sensor>();
            
            BindingContext = this;
            
            SensorTypePicker.SelectedIndex = 0;
            DataIntervalPicker.SelectedIndex = 4; // 1 hour
            
            LoadSensorsAsync();
        }

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

        private void OnSensorTypeChanged(object sender, EventArgs e)
        {
            FilterSensors();
        }

        private void OnSensorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                SelectedSensor = e.CurrentSelection[0] as Sensor;
            }
            else
            {
                SelectedSensor = null;
            }
        }

        private async void OnAddSensorClicked(object sender, EventArgs e)
        {
            string sensorName = await DisplayPromptAsync("Add Sensor", "Enter sensor name:", initialValue: "New Sensor");
            if (string.IsNullOrWhiteSpace(sensorName))
                return;
                
            string[] types = ["Air Quality", "Water Quality", "Weather"];
            string type = await DisplayActionSheet("Select sensor type:", "Cancel", null, types);
            if (type == "Cancel" || string.IsNullOrWhiteSpace(type))
                return;
                
            await DisplayAlert("Sensor Added", $"Sensor '{sensorName}' of type '{type}' would be added in a real implementation.", "OK");
            
            await LoadSensorsAsync();
        }

        private async void OnEditSensorClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string newName = await DisplayPromptAsync("Edit Sensor", "Update sensor name:", 
                initialValue: SelectedSensor.Name);
                
            if (string.IsNullOrWhiteSpace(newName) || newName == SelectedSensor.Name)
                return;
                
            SelectedSensor.Name = newName;
            
            await DisplayAlert("Sensor Updated", $"Sensor name updated to '{newName}'.", "OK");
            
            OnPropertyChanged(nameof(SelectedSensor));
            SensorListView.ItemsSource = null;
            SensorListView.ItemsSource = Sensors;
        }

        private async void OnUpdateStatusClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string[] statuses = new[] { "operational", "maintenance", "offline" };
            string newStatus = await DisplayActionSheet("Update status:", "Cancel", null, statuses);
            
            if (newStatus == "Cancel" || string.IsNullOrWhiteSpace(newStatus) || newStatus == SelectedSensor.Status)
                return;
                
            SelectedSensor.Status = newStatus;
            
            await DisplayAlert("Status Updated", $"Sensor status updated to '{newStatus}'.", "OK");
            
            OnPropertyChanged(nameof(SelectedSensor));
            SensorListView.ItemsSource = null;
            SensorListView.ItemsSource = Sensors;
        }

        private async void OnUpdateFirmwareClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string currentVersion = SelectedSensor.FirmwareVersion;
            double versionNum = double.Parse(currentVersion.Split('.')[2]) + 1;
            string newVersion = $"{currentVersion.Split('.')[0]}.{currentVersion.Split('.')[1]}.{versionNum}";
            
            bool confirm = await DisplayAlert("Update Firmware", 
                $"Are you sure you want to update firmware from {currentVersion} to {newVersion}?", 
                "Yes", "No");
                
            if (!confirm)
                return;
                
            await DisplayAlert("Firmware Update", "Firmware update process initiated. This would trigger an OTA update to the device in a real implementation.", "OK");
            
            SelectedSensor.FirmwareVersion = newVersion;
            OnPropertyChanged(nameof(SelectedSensor));
        }

        private void OnDataIntervalChanged(object sender, EventArgs e)
        {
            _dataCollectionInterval = DataIntervalPicker.SelectedItem?.ToString() ?? "1 hour";
        }

        private void OnAutoCalibrationToggled(object sender, ToggledEventArgs e)
        {
            _autoCalibration = e.Value;
        }

        private void OnAlertNotificationsToggled(object sender, ToggledEventArgs e)
        {
            _alertNotifications = e.Value;
        }

        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Settings Saved", 
                $"Data Collection Interval: {_dataCollectionInterval}\n" +
                $"Auto-Calibration: {(_autoCalibration ? "Enabled" : "Disabled")}\n" +
                $"Alert Notifications: {(_alertNotifications ? "Enabled" : "Disabled")}", 
                "OK");
        }

        private async Task LoadSensorsAsync()
        {
            try
            {
                _allSensors = await _sensorService.GetSensorsAsync();
                FilterSensors();
                SensorListView.ItemsSource = Sensors;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
        }

        private void FilterSensors()
        {
            if (_allSensors == null)
                return;

            string selectedType = SensorTypePicker.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(selectedType) || selectedType == "All Types")
            {
                Sensors = new ObservableCollection<Sensor>(_allSensors);
            }
            else
            {
                Sensors = new ObservableCollection<Sensor>(
                    _allSensors.Where(s => s.Type == selectedType)
                );
            }
            
            SensorListView.ItemsSource = Sensors;
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}