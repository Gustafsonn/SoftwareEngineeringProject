using Microsoft.Maui.Controls;

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
            await Shell.Current.GoToAsync("//Administrator/Dashboard");
        }

        private async void OnDataClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/DataStoragePage");
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SettingsPage");
        }
        
        private async void OnUserManagementClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/UserManagementPage");
        }
    }
}