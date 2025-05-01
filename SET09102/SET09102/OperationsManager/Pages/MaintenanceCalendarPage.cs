using SET09102.Services;

namespace SET09102.OperationsManager.Pages
{
    internal class MaintenanceCalendarPage : Page
    {
        private IMaintenanceSchedulingService maintenanceService;

        public MaintenanceCalendarPage(IMaintenanceSchedulingService maintenanceService)
        {
            this.maintenanceService = maintenanceService;
        }
    }
}