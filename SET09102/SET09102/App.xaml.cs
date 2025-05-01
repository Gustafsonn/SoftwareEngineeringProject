using Microsoft.Maui.Controls;
using SET09102.Services;

namespace SET09102
{
    public partial class App : Application
    {
        public App(IAuthService authService)
        {
            // Do NOT call InitializeComponent() to avoid errors
            
            // Set the main page to AppShell, passing the auth service
            MainPage = new AppShell(authService);
        }
    }
}