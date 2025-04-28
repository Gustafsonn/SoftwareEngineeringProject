using Microsoft.Maui.Controls;
using SET09102.Services;
using SET09102.Administrator.Pages;

namespace SET09102
{
    public partial class AppShell : Shell
    {
        private readonly IAuthService _authService;

        public AppShell(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Register routes for role-specific pages
            Routing.RegisterRoute("//MainPage", typeof(MainPage));
            Routing.RegisterRoute("//Administrator/Login", typeof(LoginPage));
            Routing.RegisterRoute("//Administrator/Dashboard", typeof(Administrator.Pages.MainPage));
            Routing.RegisterRoute("//Administrator/DataStoragePage", typeof(Administrator.Pages.DataStoragePage));
            Routing.RegisterRoute("//Administrator/SettingsPage", typeof(Administrator.Pages.SettingsPage));
            Routing.RegisterRoute("//Administrator/SensorConfigurationPage", typeof(Administrator.Pages.SensorConfigurationPage));
            Routing.RegisterRoute("//OperationsManager/MainPage", typeof(OperationsManager.Pages.MainPage));
            Routing.RegisterRoute("//OperationsManager/DataVerificationPage", typeof(OperationsManager.Pages.DataVerificationPage));
            Routing.RegisterRoute("//EnvironmentalScientist/MainPage", typeof(EnvironmentalScientist.Pages.MainPage));
            Routing.RegisterRoute("//EnvironmentalScientist/MapPage", typeof(EnvironmentalScientist.Pages.MapPage));
            Routing.RegisterRoute("//EnvironmentalScientist/HistoricalData", typeof(EnvironmentalScientist.Pages.HistoricalDataPage));
            Routing.RegisterRoute("//EnvironmentalScientist/EnvTrendPage", typeof(EnvironmentalScientist.Pages.EnvTrendPage));

            Navigating += OnNavigating;
        }

        private async void OnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.Target.Location.ToString().StartsWith("//Administrator/"))
            {
                if (e.Target.Location.ToString() == "//Administrator/Login")
                    return;

                bool isAuthenticated = await _authService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    e.Cancel();
                    await Shell.Current.GoToAsync("//Administrator/Login");
                }
            }
        }
    }
}
