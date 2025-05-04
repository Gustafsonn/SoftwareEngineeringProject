using System.Collections.ObjectModel;
using SET09102.Models;
using SET09102.Services;
using SET09102.Common.Contracts;

namespace SET09102.EnvironmentalScientist.Pages
{
    public partial class DisplayThresholdAlerts : ContentPage
    {
        private readonly ISensorService _sensorService;
        private readonly IMapService _mapService;
        private ObservableCollection<SensorAlert> _sensorAlerts = [];
        private SensorAlert? _selectedSensorAlert = null;
        private string _mapUrl = string.Empty;

        public DisplayThresholdAlerts(ISensorService sensorService, IMapService mapService)
        {
            _sensorService = sensorService;
            _mapService = mapService;
            MapUrl = mapService.GetDefaultUrl();
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
                MapUrl =
                    _selectedSensorAlert == null
                        ? _mapService.GetDefaultUrl()
                        : _mapService.GetMapUrl(_selectedSensorAlert.Sensor.Latitude, _selectedSensorAlert.Sensor.Longitude);
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