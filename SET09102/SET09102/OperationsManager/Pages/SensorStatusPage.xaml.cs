using SET09102.OperationsManager.Models;
using SET09102.OperationsManager.Services;

namespace SET09102.OperationsManager.Pages
{
    public partial class SensorStatusPage : ContentPage
    {
        private readonly ISensorMonitoringService _sensorMonitoringService;
        private List<SensorStatusInfo> _allSensorStatuses;

        public SensorStatusPage(ISensorMonitoringService sensorMonitoringService)
        {
            InitializeComponent();
            _sensorMonitoringService = sensorMonitoringService;
            BindingContext = this;
            LoadSensorStatuses();
        }

        private async void LoadSensorStatuses()
        {
            try
            {
                _allSensorStatuses = (await _sensorMonitoringService.GetAllSensorStatusesAsync()).ToList();
                SensorStatusCollection.ItemsSource = _allSensorStatuses;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load sensor statuses: {ex.Message}", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                SensorStatusCollection.ItemsSource = _allSensorStatuses;
            }
            else
            {
                SensorStatusCollection.ItemsSource = _allSensorStatuses
                    .Where(s => s.SensorName.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase) ||
                               s.Location.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private async void OnSensorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SensorStatusInfo selectedSensor)
            {
                await Navigation.PushAsync(new SensorDetailPage(selectedSensor));
            }
        }
    }
} 