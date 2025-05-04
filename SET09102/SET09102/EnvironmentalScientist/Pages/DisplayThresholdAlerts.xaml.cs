using System.Collections.ObjectModel;
using SET09102.Models;
using SET09102.Services;

namespace SET09102.EnvironmentalScientist.Pages
{
    public partial class DisplayThresholdAlerts : ContentPage
    {
        private readonly ISensorService _sensorService;
        private ObservableCollection<SensorAlert> _sensorAlerts = [];
        private SensorAlert? _selectedSensorAlert = null;
        private string _mapUrl = "https://maps.google.com/maps";

        public DisplayThresholdAlerts(ISensorService sensorService)
        {
            _sensorService = sensorService;
            InitializeComponent();
            BindingContext = this;  // Set the binding context for data binding
            _ = LoadSensorsAlertsAsync();            
        }


        public ObservableCollection<SensorAlert> SensorAlerts
        {
            get => _sensorAlerts;
            set
            {
                _sensorAlerts = value;
                OnPropertyChanged();
            }
        }

        public SensorAlert? SelectedSensorAlert
        {
            get => _selectedSensorAlert;
            set
            {
                _selectedSensorAlert = value;
                if (_selectedSensorAlert == null)
                {
                    MapUrl = "https://maps.google.com/maps";
                }
                else
                {
                    MapUrl = $"https://maps.google.com/maps?q={_selectedSensorAlert.Sensor.Latitude},{_selectedSensorAlert.Sensor.Longitude}";
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(SensorAlertSelected));
                OnPropertyChanged(nameof(NoSensorAlertSelected));
                OnPropertyChanged(nameof(SelectedSensorHeading));
            }
        }

        public string MapUrl
        {
            get => _mapUrl;
            set
            {
                _mapUrl = value;
                OnPropertyChanged();

            }
        }

        public bool SensorAlertSelected => _selectedSensorAlert != null;

        public bool NoSensorAlertSelected => _selectedSensorAlert == null;

        public string SelectedSensorHeading => _selectedSensorAlert == null ? string.Empty : $"{_selectedSensorAlert.Sensor.Name} - {_selectedSensorAlert.Sensor.Location}";


        private async Task LoadSensorsAlertsAsync()
        {
            SensorAlerts = await _sensorService.GetSensorAlertsAsync();
        }
    }
}