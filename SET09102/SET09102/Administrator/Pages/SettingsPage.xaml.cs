using Microsoft.Maui.Controls;
using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Administrator.Pages
{
    /// <summary>
    /// Represents the settings page for the Administrator section.
    /// Allows configuration of global settings and management of individual sensors.
    /// Implements INotifyPropertyChanged to support data binding updates.
    /// </summary>
    public partial class SettingsPage : ContentPage, INotifyPropertyChanged
    {
        /// <summary>
        /// Service for interacting with sensor data storage.
        /// </summary>
        private readonly SensorService _sensorService;
        /// <summary>
        /// The complete list of sensors loaded from the service.
        /// </summary>
        private ObservableCollection<Sensor> _allSensors;
        /// <summary>
        /// The filtered list of sensors currently displayed in the ListView.
        /// </summary>
        private ObservableCollection<Sensor> _filteredSensors;
        /// <summary>
        /// The currently selected sensor in the ListView.
        /// </summary>
        private Sensor _selectedSensor;
        /// <summary>
        /// Flag indicating whether a sensor is currently selected. Used to enable/disable context-specific buttons.
        /// </summary>
        private bool _hasSelectedSensor;
        
        /// <summary>
        /// Stores the selected data collection interval setting.
        /// </summary>
        private string _dataCollectionInterval = "1 hour";
        /// <summary>
        /// Stores the state of the auto-calibration setting.
        /// </summary>
        private bool _autoCalibration = true;
        /// <summary>
        /// Stores the state of the alert notifications setting.
        /// </summary>
        private bool _alertNotifications = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// Sets up dependencies, initializes collections, sets the binding context,
        /// configures default picker selections, and initiates sensor loading.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            
            string dbPath = new DatabaseService().GetDatabasePath();
            _sensorService = new SensorService(dbPath);
            
            _allSensors = new ObservableCollection<Sensor>();
            _filteredSensors = new ObservableCollection<Sensor>();
            
            BindingContext = this;
            
            // Set default selections for pickers
            SensorTypePicker.SelectedIndex = 0; // "All Types"
            DataIntervalPicker.SelectedIndex = 4; // "1 hour"
            
            _ = LoadSensorsAsync();
        }

        /// <summary>
        /// Gets or sets the collection of sensors currently displayed (filtered).
        /// </summary>
        /// <remarks>
        /// Raises the PropertyChanged event when set.
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
        /// Gets or sets the currently selected sensor in the list.
        /// </summary>
        /// <remarks>
        /// Updates the HasSelectedSensor property and raises the PropertyChanged event when set.
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
        /// <remarks>
        /// Used to control the enabled state of sensor-specific action buttons.
        /// Raises the PropertyChanged event when set.
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
        /// Handles the event when the selected sensor type changes in the picker.
        /// Triggers the filtering of the sensor list.
        /// </summary>
        /// <param name="sender">The object that raised the event (SensorTypePicker).</param>
        /// <param name="e">Event arguments.</param>
        private void OnSensorTypeChanged(object sender, EventArgs e)
        {
            FilterSensors();
        }

        /// <summary>
        /// Handles the event when the selection changes in the sensor ListView.
        /// Updates the SelectedSensor property based on the current selection.
        /// </summary>
        /// <param name="sender">The object that raised the event (SensorListView).</param>
        /// <param name="e">Event arguments containing the selection changes.</param>
        private void OnSensorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                SelectedSensor = e.CurrentSelection[0] as Sensor;
            }
            else
            {
                SelectedSensor = null; // Clear selection if nothing is selected
            }
        }

        /// <summary>
        /// Handles the click event for the "Add Sensor" button.
        /// Prompts the user for a sensor name and type, then simulates adding the sensor
        /// (in a real app, this would interact with the service) and reloads the list.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnAddSensorClicked(object sender, EventArgs e)
        {
            string sensorName = await DisplayPromptAsync("Add Sensor", "Enter sensor name:", initialValue: "New Sensor");
            if (string.IsNullOrWhiteSpace(sensorName))
                return;
                
            string[] types = ["Air Quality", "Water Quality", "Weather"];
            string type = await DisplayActionSheet("Select sensor type:", "Cancel", null, types);
            if (type == "Cancel" || string.IsNullOrWhiteSpace(type))
                return; 
                
            // Simulate adding the sensor
            await DisplayAlert("Sensor Added", $"Sensor '{sensorName}' of type '{type}' would be added in a real implementation.", "OK");
            
            // Reload the sensor list to reflect changes
            await LoadSensorsAsync();
        }

        /// <summary>
        /// Handles the click event for the "Edit Sensor" button.
        /// Prompts the user to update the name of the currently selected sensor.
        /// Updates the sensor object and refreshes the ListView.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnEditSensorClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string newName = await DisplayPromptAsync("Edit Sensor", "Update sensor name:", 
                initialValue: SelectedSensor.Name);
                
            if (string.IsNullOrWhiteSpace(newName) || newName == SelectedSensor.Name)
                return;
                
            // Update the sensor's name
            SelectedSensor.Name = newName;
            
            await DisplayAlert("Sensor Updated", $"Sensor name updated to '{newName}'.", "OK");
            
            OnPropertyChanged(nameof(SelectedSensor)); 
            SensorListView.ItemsSource = null; 
            SensorListView.ItemsSource = Sensors;
        }

        /// <summary>
        /// Handles the click event for the "Update Status" button.
        /// Prompts the user to select a new status for the currently selected sensor.
        /// Updates the sensor object and refreshes the ListView.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnUpdateStatusClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return;
                
            string[] statuses = new[] { "operational", "maintenance", "offline" };
            string newStatus = await DisplayActionSheet("Update status:", "Cancel", null, statuses);
            
            if (newStatus == "Cancel" || string.IsNullOrWhiteSpace(newStatus) || newStatus == SelectedSensor.Status)
                return;
                
            // Update the sensor's status 
            SelectedSensor.Status = newStatus;
            
            await DisplayAlert("Status Updated", $"Sensor status updated to '{newStatus}'.", "OK");
            
            // Notify UI and refresh ListView
            OnPropertyChanged(nameof(SelectedSensor));
            SensorListView.ItemsSource = null;
            SensorListView.ItemsSource = Sensors;
        }

        /// <summary>
        /// Handles the click event for the "Update Firmware" button.
        /// Simulates a firmware update process for the selected sensor by incrementing the patch version.
        /// Prompts for confirmation before proceeding.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnUpdateFirmwareClicked(object sender, EventArgs e)
        {
            if (SelectedSensor == null)
                return; // No sensor selected
                
            // Basic firmware version increment logic (example)
            string currentVersion = SelectedSensor.FirmwareVersion;
            string newVersion = currentVersion;
            try 
            {
                 var versionParts = currentVersion.Split('.');
                 if (versionParts.Length == 3 && int.TryParse(versionParts[2], out int patch))
                 {
                     newVersion = $"{versionParts[0]}.{versionParts[1]}.{patch + 1}";
                 }
            }
            catch (Exception) { /* Ignore parsing errors */ }

            bool confirm = await DisplayAlert("Update Firmware", 
                $"Are you sure you want to update firmware from {currentVersion} to {newVersion}?", 
                "Yes", "No");
                
            if (!confirm)
                return; 
                
            await DisplayAlert("Firmware Update", "Firmware update process initiated. This would trigger an OTA update to the device in a real implementation.", "OK");
            
            // Update the sensor's firmware version
            SelectedSensor.FirmwareVersion = newVersion;
            OnPropertyChanged(nameof(SelectedSensor)); 
            SensorListView.ItemsSource = null;
            SensorListView.ItemsSource = Sensors;
        }

        /// <summary>
        /// Handles the event when the selected data collection interval changes in the picker.
        /// Updates the internal setting variable.
        /// </summary>
        /// <param name="sender">The object that raised the event (DataIntervalPicker).</param>
        /// <param name="e">Event arguments.</param>
        private void OnDataIntervalChanged(object sender, EventArgs e)
        {
            _dataCollectionInterval = DataIntervalPicker.SelectedItem?.ToString() ?? "1 hour"; // Update setting
        }

        /// <summary>
        /// Handles the event when the auto-calibration switch is toggled.
        /// Updates the internal setting variable.
        /// </summary>
        /// <param name="sender">The object that raised the event (Switch).</param>
        /// <param name="e">Event arguments containing the new toggle state.</param>
        private void OnAutoCalibrationToggled(object sender, ToggledEventArgs e)
        {
            _autoCalibration = e.Value;
        }

        /// <summary>
        /// Handles the event when the alert notifications switch is toggled.
        /// Updates the internal setting variable.
        /// </summary>
        /// <param name="sender">The object that raised the event (Switch).</param>
        /// <param name="e">Event arguments containing the new toggle state.</param>
        private void OnAlertNotificationsToggled(object sender, ToggledEventArgs e)
        {
            _alertNotifications = e.Value;
        }

        /// <summary>
        /// Handles the click event for the "Save Settings" button.
        /// Displays a confirmation alert showing the currently selected global settings.
        /// Note: This method currently only displays the settings; it doesn't persist them.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            // these settings would be saved to a persistent store
            await DisplayAlert("Settings Saved", 
                $"Data Collection Interval: {_dataCollectionInterval}\n" +
                $"Auto-Calibration: {(_autoCalibration ? "Enabled" : "Disabled")}\n" +
                $"Alert Notifications: {(_alertNotifications ? "Enabled" : "Disabled")}", 
                "OK");
        }

        /// <summary>
        /// Asynchronously loads all sensors from the sensor service, applies the current filter,
        /// and updates the ItemsSource for the sensor list view.
        /// Handles potential errors during sensor loading.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadSensorsAsync()
        {
            try
            {
                _allSensors = await _sensorService.GetSensorsAsync();
                FilterSensors(); 
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Filters the main sensor list (_allSensors) based on the selected type in the SensorTypePicker
        /// and updates the 'Sensors' collection (which is bound to the ListView).
        /// Also updates the ListView's ItemsSource directly.
        /// </summary>
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

        /// <summary>
        /// Event raised when a property value changes, to notify the UI.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. Automatically determined by the compiler if not provided.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}