using SET09102.Models;

namespace SET09102.OperationsManager.Pages
{
    internal class SensorDetailPage : Page
    {
        private SensorStatusInfo selectedSensor;

        public SensorDetailPage(SensorStatusInfo selectedSensor)
        {
            this.selectedSensor = selectedSensor;
        }
    }
}