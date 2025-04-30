using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    public partial class SensorMonitoringPage : ContentPage, INotifyPropertyChanged
    {
        private readonly SensorService _sensorService;
        private readonly SensorSettingsService _sensorSettingsService;
        private ObservableCollection<Sensor> _allSensors = new();
        private ObservableCollection<Sensor> _filteredSensors = new();
        private Sensor _selectedSensor;
        private bool _hasSelectedSensor;
        private string _selectedSensorType = "All Types";
        private string _selectedStatus = "All Status";
        private bool _showActiveOnly = true;
        
        // Status counts for dashboard
        private int _operationalCount;
        private int _maintenanceCount;
        private int _offlineCount;

        public SensorMonitoringPage()
        {
            try
            {
                InitializeComponent();
                
                string dbPath = new DatabaseService().GetDatabasePath();
                _sensorService = new SensorService(dbPath);
                _sensorSettingsService = new SensorSettingsService(new DatabaseService());
                
                BindingContext = this;
                
                // Set initial picker values
                SensorTypePicker.SelectedIndex = 0;
                StatusPicker.SelectedIndex = 0;
                ActiveOnlyCheckbox.IsChecked = true;
                
                // Load sensors
                LoadSensorsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SensorMonitoringPage: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                
                DisplayAlert("Error", "There was a problem loading the sensor monitoring page. Please try again later.", "OK");
            }
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
                UpdateSensorDetailPanel();
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

        private async void LoadSensorsAsync()
        {
            try
            {
                _allSensors = await _sensorService.GetSensorsAsync();
                
                // Make sure _allSensors is never null to avoid crashes
                if (_allSensors == null)
                    _allSensors = new ObservableCollection<Sensor>();
                
                FilterSensors();
                UpdateStatusCounts();
                SensorListView.ItemsSource = Sensors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadSensorsAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                // Set empty collections to avoid null reference exceptions
                _allSensors = new ObservableCollection<Sensor>();
                Sensors = new ObservableCollection<Sensor>();
                
                await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
        }

        private void FilterSensors()
        {
            if (_allSensors == null)
                return;

            // Apply type filter
            var filtered = _allSensors.AsEnumerable();
            
            if (_selectedSensorType != "All Types")
            {
                filtered = filtered.Where(s => s.Type == _selectedSensorType);
            }
            
            // Apply status filter
            if (_selectedStatus != "All Status")
            {
                filtered = filtered.Where(s => s.Status.ToLower() == _selectedStatus.ToLower());
            }
            
            // Apply active only filter
            if (_showActiveOnly)
            {
                filtered = filtered.Where(s => s.IsActive);
            }
            
            Sensors = new ObservableCollection<Sensor>(filtered);
            
            // Update the status counts after filtering
            UpdateStatusCounts();
        }
        
        private void UpdateStatusCounts()
        {
            // Get status counts from all sensors (not filtered)
            var sensorsToCount = _showActiveOnly 
                ? _allSensors.Where(s => s.IsActive) 
                : _allSensors;
                
            _operationalCount = sensorsToCount.Count(s => s.Status.ToLower() == "operational");
            _maintenanceCount = sensorsToCount.Count(s => s.Status.ToLower() == "maintenance");
            _offlineCount = sensorsToCount.Count(s => s.Status.ToLower() == "offline");
            
            // Update the UI labels
            OperationalCountLabel.Text = _operationalCount.ToString();
            MaintenanceCountLabel.Text = _maintenanceCount.ToString();
            OfflineCountLabel.Text = _offlineCount.ToString();
        }
        
        private void UpdateSensorDetailPanel()
        {
            if (SelectedSensor != null)
            {
                DetailIdLabel.Text = SelectedSensor.Id.ToString();
                DetailNameLabel.Text = SelectedSensor.Name;
                DetailTypeLabel.Text = SelectedSensor.Type;
                DetailLocationLabel.Text = SelectedSensor.Location;
                DetailStatusLabel.Text = SelectedSensor.Status;
                DetailLastCalibrationLabel.Text = SelectedSensor.LastCalibration.ToString("yyyy-MM-dd");
                DetailNextCalibrationLabel.Text = SelectedSensor.NextCalibration.ToString("yyyy-MM-dd");
                DetailFirmwareLabel.Text = SelectedSensor.FirmwareVersion;
                
                // Show the detail panel
                SensorDetailPanel.IsVisible = true;
            }
            else
            {
                SensorDetailPanel.IsVisible = false;
            }
        }

        private void OnSensorTypeChanged(object sender, EventArgs e)
        {
            _selectedSensorType = SensorTypePicker.SelectedItem?.ToString() ?? "All Types";
            FilterSensors();
        }

        private void OnStatusChanged(object sender, EventArgs e)
        {
            _selectedStatus = StatusPicker.SelectedItem?.ToString() ?? "All Status";
            FilterSensors();
        }

        private void OnActiveOnlyChanged(object sender, CheckedChangedEventArgs e)
        {
            _showActiveOnly = e.Value;
            FilterSensors();
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadSensorsAsync();
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

        private void OnCloseSensorDetailClicked(object sender, EventArgs e)
        {
            // Clear the selection in the CollectionView and hide the detail panel
            SensorListView.SelectedItem = null;
            SelectedSensor = null;
            SensorDetailPanel.IsVisible = false;
        }

        private async void OnChangeStatusClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string[] statuses = new[] { "operational", "maintenance", "offline" };
            string newStatus = await DisplayActionSheet("Update status:", "Cancel", null, statuses);
            
            if (newStatus == "Cancel" || string.IsNullOrWhiteSpace(newStatus) || newStatus == SelectedSensor.Status)
                return;
                
            // Update the sensor status
            SelectedSensor.Status = newStatus;
            
            // In a real application, you would call a service to update the status in the database
            // For now, we'll just update the UI
            DetailStatusLabel.Text = newStatus;
            
            await DisplayAlert("Status Updated", $"Sensor status updated to '{newStatus}'.", "OK");
            
            // Refresh the list to show the updated status
            FilterSensors();
            UpdateStatusCounts();
        }

        private async void OnUpdateFirmwareClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string currentVersion = SelectedSensor.FirmwareVersion;
            double versionNum = double.Parse(currentVersion.Split('.')[2]) + 0.1;
            string newVersion = $"{currentVersion.Split('.')[0]}.{currentVersion.Split('.')[1]}.{versionNum}";
            
            bool confirm = await DisplayAlert("Update Firmware", 
                $"Are you sure you want to update firmware from {currentVersion} to {newVersion}?", 
                "Yes", "No");
                
            if (!confirm)
                return;
                
            await DisplayAlert("Firmware Update", "Firmware update process initiated. This would trigger an OTA update to the device in a real implementation.", "OK");
            
            // Update the firmware version
            SelectedSensor.FirmwareVersion = newVersion;
            DetailFirmwareLabel.Text = newVersion;
        }

        private async void OnConfigureSensorClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            try
            {
                // Create a new instance of the SensorSettingsDialog
                var settingsDialog = new SensorSettingsDialog(new DatabaseService(), SelectedSensor);
                
                // Navigate to the settings dialog
                await Navigation.PushModalAsync(settingsDialog);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to open sensor settings: {ex.Message}", "OK");
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}