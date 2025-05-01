using SET09102.Services;

namespace SET09102.OperationsManager.Pages
{
    internal class MaintenanceDetailPage : Page
    {
        private MaintenanceSchedule selectedSchedule;
        private IMaintenanceSchedulingService maintenanceService;

        public MaintenanceDetailPage(MaintenanceSchedule selectedSchedule, IMaintenanceSchedulingService maintenanceService)
        {
            this.selectedSchedule = selectedSchedule;
            this.maintenanceService = maintenanceService;
        }
    }
}