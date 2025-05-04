using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SET09102.OperationsManager.Pages
{
    /// <summary>
    /// Page for scheduling and managing maintenance activities for environmental monitoring sensors.
    /// </summary>
    /// <remarks>
    /// This page provides functionality to:
    /// <list type="bullet">
    ///   <item><description>View sensors that need maintenance</description></item>
    ///   <item><description>Schedule maintenance activities</description></item>
    ///   <item><description>Log completed maintenance</description></item>
    ///   <item><description>View maintenance history</description></item>
    /// </list>
    /// </remarks>
    public partial class MaintenanceSchedulePage : ContentPage, INotifyPropertyChanged
    {
        private readonly ISensorService _sensorService;
        private readonly SensorSettingsService _settingsService;
        private ObservableCollection<Sensor> _sensors = [];
        private ObservableCollection<MaintenanceLog> _maintenanceLogs = [];
        private Sensor? _selectedSensor;
        private bool _hasSelectedSensor;
        
        /// <summary>
        /// Command to schedule maintenance for a sensor.
        /// </summary>
        public ICommand ScheduleMaintenanceCommand { get; }
        
        /// <summary>
        /// Command to perform and log maintenance for a sensor.
        /// </summary>
        public ICommand PerformMaintenanceCommand { get; }

        /// <summary>
        /// Service responsible for handling Maintenance Logs
        /// </summary>
        private readonly MaintenanceLogService _maintenanceLogService;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="MaintenanceSchedulePage"/> class.
        /// </summary>
        /// <param name="sensorService">Service for accessing sensor data.</param>
        /// <param name="settingsService">Service for accessing sensor settings.</param>
        public MaintenanceSchedulePage(ISensorService sensorService, SensorSettingsService settingsService, MaintenanceLogService maintenanceLogService)
        {
            _sensorService = sensorService;
            _settingsService = settingsService;
            _maintenanceLogService = maintenanceLogService;
    
            ScheduleMaintenanceCommand = new Command<Sensor>(OnScheduleMaintenance);
            PerformMaintenanceCommand = new Command<Sensor>(OnPerformMaintenance);
    
            InitializeComponent();
            BindingContext = this;
            _ = LoadSensorsAsync();
        }
        
        /// <summary>
        /// Gets or sets the collection of sensors that need maintenance.
        /// </summary>
        /// <value>
        /// A collection of sensors requiring maintenance or calibration.
        /// </value>
        public ObservableCollection<Sensor> Sensors
        {
            get => _sensors;
            set
            {
                _sensors = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the maintenance logs for the selected sensor.
        /// </summary>
        /// <value>
        /// A collection of maintenance logs.
        /// </value>
        public ObservableCollection<MaintenanceLog> MaintenanceLogs
        {
            get => _maintenanceLogs;
            set
            {
                _maintenanceLogs = value;
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
        /// When a sensor is selected, the <see cref="MaintenanceLogs"/> property is automatically updated
        /// with the maintenance history for that sensor.
        /// </remarks>
        public Sensor SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                if (_selectedSensor != value)
                {
                    _selectedSensor = value;
                    HasSelectedSensor = (_selectedSensor != null);
                    OnPropertyChanged();
                    _ = LoadMaintenanceLogsAsync();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether a sensor is currently selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if a sensor is selected; otherwise, <c>false</c>.
        /// </value>
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
        /// Loads sensors that need maintenance or calibration from the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method filters sensors to include only those that:
        /// <list type="bullet">
        ///   <item><description>Are currently active</description></item>
        ///   <item><description>Are in maintenance status</description></item>
        ///   <item><description>Have upcoming maintenance within 7 days</description></item>
        ///   <item><description>Have upcoming calibration within 7 days</description></item>
        /// </list>
        /// </remarks>
        private async Task LoadSensorsAsync()
        {
            try
            {
                var allSensors = await _sensorService.GetSensorsAsync();
                
                // Filter to only show sensors that need maintenance or calibration
                var sensorsNeedingAttention = allSensors.Where(s => 
                    s.IsActive && (
                        s.Status.ToLower() == "maintenance" ||
                        s.NextMaintenance <= DateTime.Now.AddDays(7) ||
                        s.NextCalibration <= DateTime.Now.AddDays(7)
                    )).ToList();
                
                Sensors = new ObservableCollection<Sensor>(sensorsNeedingAttention);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Loads maintenance logs for the currently selected sensor.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// If no sensor is selected, the <see cref="MaintenanceLogs"/> collection will be empty.
        /// </remarks>
        private async Task LoadMaintenanceLogsAsync()
        {
            if (SelectedSensor == null)
            {
                MaintenanceLogs = new ObservableCollection<MaintenanceLog>();
                return;
            }
    
            try
            {
                MaintenanceLogs = await _maintenanceLogService.GetMaintenanceLogsAsync(SelectedSensor.Id);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load maintenance logs: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Handles the action to schedule maintenance for a sensor.
        /// </summary>
        /// <param name="sensor">The sensor to schedule maintenance for.</param>
        /// <remarks>
        /// This method prompts the user to enter a date for the scheduled maintenance
        /// and updates the sensor's next maintenance date in the database.
        /// </remarks>
        private async void OnScheduleMaintenance(Sensor sensor)
        {
            if (sensor == null) return;
            
            try
            {
                // Let the user select a date for maintenance
                DateTime suggestedDate = DateTime.Now.AddDays(3);
                string result = await DisplayPromptAsync(
                    "Schedule Maintenance",
                    $"When should maintenance be performed for {sensor.Name}?",
                    initialValue: suggestedDate.ToString("yyyy-MM-dd"),
                    maxLength: 10,
                    keyboard: Keyboard.Text
                );
                
                if (string.IsNullOrEmpty(result)) return;
                
                if (DateTime.TryParse(result, out DateTime maintenanceDate))
                {
                    // Update the sensor's next maintenance date
                    var updatedSensor = sensor.Clone();
                    updatedSensor.NextMaintenance = maintenanceDate;
                    await _sensorService.UpdateSensorAsync(updatedSensor);
                    
                    await DisplayAlert("Success", $"Maintenance scheduled for {sensor.Name} on {maintenanceDate:yyyy-MM-dd}", "OK");
                    await LoadSensorsAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Invalid date format. Please use YYYY-MM-DD format.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to schedule maintenance: {ex.Message}", "OK");
            }
        }
        
        /// <summary>
        /// Handles the action to perform and log maintenance for a sensor.
        /// </summary>
        /// <param name="sensor">The sensor on which maintenance is performed.</param>
        /// <remarks>
        /// This method:
        /// <list type="bullet">
        ///   <item><description>Prompts for maintenance notes</description></item>
        ///   <item><description>Updates the sensor's last and next maintenance dates</description></item>
        ///   <item><description>Sets sensor status to operational if it was in maintenance</description></item>
        ///   <item><description>Logs the maintenance activity</description></item>
        /// </list>
        /// </remarks>
        private async void OnPerformMaintenance(Sensor sensor)
        {
            if (sensor == null) return;
            
            try
            {
                // Prompt for maintenance notes
                string notes = await DisplayPromptAsync(
                    "Perform Maintenance",
                    $"Enter maintenance notes for {sensor.Name}:",
                    placeholder: "Maintenance details",
                    maxLength: 500
                );
                
                if (string.IsNullOrEmpty(notes)) return;
                
                // Set last maintenance to now and schedule next maintenance based on settings
                var updatedSensor = sensor.Clone();
                updatedSensor.LastMaintenance = DateTime.Now;
                
                // Retrieve sensor settings for maintenance interval
                var settings = await _settingsService.GetSensorSettingsAsync(sensor.Id);
                int maintenanceIntervalDays = 90; // Default to 90 days
                
                updatedSensor.NextMaintenance = DateTime.Now.AddDays(maintenanceIntervalDays);
                
                // If the sensor was in maintenance status, set it back to operational
                if (updatedSensor.Status.ToLower() == "maintenance")
                {
                    updatedSensor.Status = "operational";
                }
                
                await _sensorService.UpdateSensorAsync(updatedSensor);
                
                var maintenanceLog = new MaintenanceLog
                {
                    SensorId = sensor.Id,
                    MaintenanceType = "Routine",
                    PerformedBy = "System Operator",
                    Notes = notes,
                    CreatedAt = DateTime.Now
                };
            
                await _maintenanceLogService.CreateMaintenanceLogAsync(maintenanceLog);
                
                await DisplayAlert("Success", "Maintenance completed and logged successfully", "OK");
                await LoadSensorsAsync();
                
                if (SelectedSensor?.Id == sensor.Id)
                {
                    await LoadMaintenanceLogsAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to record maintenance: {ex.Message}", "OK");
            }
        }
    }
    
    /// <summary>
    /// Represents a log entry for a maintenance activity performed on a sensor.
    /// </summary>
    public class MaintenanceLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the maintenance log.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the sensor on which maintenance was performed.
        /// </summary>
        public int SensorId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of maintenance performed.
        /// </summary>
        /// <value>
        /// A string describing the maintenance type (e.g., "Routine", "Repair", "Calibration").
        /// </value>
        public string MaintenanceType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the name or ID of the person who performed the maintenance.
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets detailed notes about the maintenance activity.
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the date and time when the maintenance was performed.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}