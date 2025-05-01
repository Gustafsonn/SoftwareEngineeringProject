using SET09102.Services;

namespace SET09102.OperationsManager.Pages
{
    internal class MalfunctionDetailPage : Page
    {
        private SensorMalfunction selectedMalfunction;
        private IMalfunctionReportingService malfunctionService;

        public MalfunctionDetailPage(SensorMalfunction selectedMalfunction, IMalfunctionReportingService malfunctionService)
        {
            this.selectedMalfunction = selectedMalfunction;
            this.malfunctionService = malfunctionService;
        }
    }
}