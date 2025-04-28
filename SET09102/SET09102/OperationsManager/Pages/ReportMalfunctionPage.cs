using SET09102.Services;

namespace SET09102.OperationsManager.Pages
{
    internal class ReportMalfunctionPage : Page
    {
        private IMalfunctionReportingService malfunctionService;

        public ReportMalfunctionPage(IMalfunctionReportingService malfunctionService)
        {
            this.malfunctionService = malfunctionService;
        }
    }
}