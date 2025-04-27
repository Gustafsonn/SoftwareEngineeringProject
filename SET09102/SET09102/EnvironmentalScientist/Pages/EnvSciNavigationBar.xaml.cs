using Microsoft.Maui.Controls;

namespace SET09102.EnvironmentalScientist.Pages
{
    public partial class EnvSciNavigationBar : ContentView
    {
        public EnvSciNavigationBar()
        {
            InitializeComponent();
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//EnvironmentalScientist/MainPage");
        }

        private async void OnMapClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//EnvironmentalScientist/MapPage");
        }
        
        private async void OnHistoricalDataClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//EnvironmentalScientist/HistoricalData");
        }
    }
}
