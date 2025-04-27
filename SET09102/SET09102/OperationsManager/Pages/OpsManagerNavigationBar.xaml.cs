using Microsoft.Maui.Controls;
using SET09102.OperationsManager.Services;

namespace SET09102.OperationsManager.Pages
{
    public partial class OpsManagerNavigationBar : ContentView
    {
        private readonly ISensorMonitoringService _sensorService;
        private readonly IMaintenanceSchedulingService _maintenanceService;
        private readonly IMalfunctionReportingService _malfunctionService;

        public OpsManagerNavigationBar(
            ISensorMonitoringService sensorService,
            IMaintenanceSchedulingService maintenanceService,
            IMalfunctionReportingService malfunctionService)
        {
            InitializeComponent();
            _sensorService = sensorService;
            _maintenanceService = maintenanceService;
            _malfunctionService = malfunctionService;
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OperationsManager/MainPage");
        }

        private async void OnSensorStatusClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OperationsManager/SensorStatusPage");
        }

        private async void OnMaintenanceClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OperationsManager/MaintenanceSchedulePage");
        }

        private async void OnMalfunctionsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OperationsManager/MalfunctionReportPage");
        }
    }
}
