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
            
            // Admin routes
            Routing.RegisterRoute("//Administrator/Login", typeof(LoginPage));
            Routing.RegisterRoute("//Administrator/Dashboard", typeof(Administrator.Pages.MainPage));
            Routing.RegisterRoute("//Administrator/DataStoragePage", typeof(Administrator.Pages.DataStoragePage));
            Routing.RegisterRoute("//Administrator/SettingsPage", typeof(Administrator.Pages.SettingsPage));
            Routing.RegisterRoute("//Administrator/UserManagementPage", typeof(Administrator.Pages.UserManagementPage));
            
            // Operations Manager routes
            Routing.RegisterRoute("//OperationsManager/MainPage", typeof(OperationsManager.Pages.MainPage));
            Routing.RegisterRoute("//OperationsManager/DataVerificationPage", typeof(OperationsManager.Pages.DataVerificationPage));
            
            // Environmental Scientist routes
            Routing.RegisterRoute("//EnvironmentalScientist/MainPage", typeof(EnvironmentalScientist.Pages.MainPage));
            Routing.RegisterRoute("//EnvironmentalScientist/MapPage", typeof(EnvironmentalScientist.Pages.MapPage));
            Routing.RegisterRoute("//EnvironmentalScientist/HistoricalData", typeof(EnvironmentalScientist.Pages.HistoricalDataPage));
            Routing.RegisterRoute("//EnvironmentalScientist/EnvTrendPage", typeof(EnvironmentalScientist.Pages.EnvTrendPage));

            Navigating += OnNavigating;
        }

        private async void OnNavigating(object sender, ShellNavigatingEventArgs e)
        {
            // Skip navigation checks if going to login page
            if (e.Target.Location.ToString() == "//Administrator/Login")
                return;
                
            // If navigating to the main page, don't need to check
            if (e.Target.Location.ToString() == "//MainPage")
                return;

            bool isAuthenticated = await _authService.IsAuthenticatedAsync();
            string userRole = await _authService.GetCurrentUserRoleAsync();

            // If not authenticated, redirect to login
            if (!isAuthenticated)
            {
                e.Cancel();
                await Shell.Current.GoToAsync("//Administrator/Login");
                return;
            }

            // Role-based access control
            string destination = e.Target.Location.ToString();

            // Check Administrator access
            if (destination.StartsWith("//Administrator/"))
            {
                if (userRole != "Administrator")
                {
                    e.Cancel();
                    await DisplayAccessDeniedAlert();
                    return;
                }
            }

            // Check Operations Manager access
            if (destination.StartsWith("//OperationsManager/"))
            {
                if (userRole != "Administrator" && userRole != "Operations Manager")
                {
                    e.Cancel();
                    await DisplayAccessDeniedAlert();
                    return;
                }
            }

            // Check Environmental Scientist access
            if (destination.StartsWith("//EnvironmentalScientist/"))
            {
                if (userRole != "Administrator" && userRole != "Environmental Scientist")
                {
                    e.Cancel();
                    await DisplayAccessDeniedAlert();
                    return;
                }
            }
        }

        private async Task DisplayAccessDeniedAlert()
        {
            await Current.DisplayAlert(
                "Access Denied", 
                "You do not have permission to access this area.", 
                "OK");
        }
 }

}

