using SET09102.Services;
using SET09102.Administrator.Pages;
using System.Diagnostics;

namespace SET09102
{
    public partial class AppShell : Shell
    {
        private readonly IAuthService _authService;

        public AppShell(IAuthService authService)
        {
            try
            {
                Debug.WriteLine("AppShell constructor started");

                InitializeComponent();
                _authService = authService;

                RegisterRoutes();
                
                Debug.WriteLine("AppShell constructor completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRASH in AppShell constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void RegisterRoutes()
        {
            try
            {
                Debug.WriteLine("Registering routes");
                
                // Main Route
                Routing.RegisterRoute("//MainPage", typeof(MainPage));

                // Administrator Routes
                Routing.RegisterRoute("//Administrator/Login", typeof(LoginPage));
                Routing.RegisterRoute("//Administrator/MainPage", typeof(Administrator.Pages.MainPage));
                Routing.RegisterRoute("//Administrator/DataStoragePage", typeof(Administrator.Pages.DataStoragePage));
                Routing.RegisterRoute("//Administrator/SensorConfigurationPage", typeof(Administrator.Pages.SensorConfigurationPage));               
                Routing.RegisterRoute("//Administrator/SensorMonitoringPage", typeof(Administrator.Pages.SensorMonitoringPage));
                Routing.RegisterRoute("//Administrator/SettingsPage", typeof(Administrator.Pages.SettingsPage));

                // Operations Manager Routes
                Routing.RegisterRoute("//OperationsManager/MainPage", typeof(OperationsManager.Pages.MainPage));
                Routing.RegisterRoute("//OperationsManager/DataVerificationPage", typeof(OperationsManager.Pages.DataVerificationPage));

                // Environmental Scientist Routes
                Routing.RegisterRoute("//EnvironmentalScientist/MainPage", typeof(EnvironmentalScientist.Pages.MainPage));
                Routing.RegisterRoute("//EnvironmentalScientist/MapPage", typeof(EnvironmentalScientist.Pages.MapPage));
                Routing.RegisterRoute("//EnvironmentalScientist/HistoricalData", typeof(EnvironmentalScientist.Pages.HistoricalDataPage));
                Routing.RegisterRoute("//EnvironmentalScientist/EnvTrendPage", typeof(EnvironmentalScientist.Pages.EnvTrendPage));

                Navigating += OnNavigating;
                
                Debug.WriteLine("Routes registered");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering routes: {ex.Message}");
                throw;
            }
        }

        private async void OnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnNavigating: {ex.Message}");
                // Don't throw here - we don't want to crash navigation
            }
        }
    }
}