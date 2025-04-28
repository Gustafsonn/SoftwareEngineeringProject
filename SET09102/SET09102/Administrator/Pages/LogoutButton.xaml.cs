using Microsoft.Maui.Controls;
using SET09102.Services;

namespace SET09102.Components
{
    public partial class LogoutButton : ContentView
    {
        private readonly IAuthService _authService;

        public LogoutButton(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await Application.Current?.MainPage.DisplayAlert(
                "Confirm Logout", 
                "Are you sure you want to log out?", 
                "Logout", "Cancel");
                
            if (confirm)
            {
                await _authService.LogoutAsync();
                await Shell.Current.GoToAsync("//Administrator/Login");
            }
        }
    }
}