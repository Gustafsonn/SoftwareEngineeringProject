using SET09102.Services;

namespace SET09102.OperationsManager.Pages
{
    internal class AddMaintenanceSchedulePage : Page
    {
        private IMaintenanceSchedulingService maintenanceService;

        public AddMaintenanceSchedulePage(IMaintenanceSchedulingService maintenanceService)
        {
            this.maintenanceService = maintenanceService;
        }
    }
}