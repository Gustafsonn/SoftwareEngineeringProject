
namespace SET09102.Administrator.Pages
{
    public partial class AdminNavigationBar : ContentView
    {
        public AdminNavigationBar()
        {
            InitializeComponent();
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/MainPage");
        }
        private async void OnSensorConfigClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SensorConfigurationPage");
        }

        private async void OnDataClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/DataStoragePage");
        }
        
        private async void OnSensorMonitorClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SensorMonitoringPage");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SettingsPage");
        }
    }
}