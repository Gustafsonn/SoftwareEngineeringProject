using SET09102.Services;

namespace SET09102.Administrator.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Clear any previous error message
            ErrorLabel.IsVisible = false;
            
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorLabel.Text = "Please enter both username and password";
                ErrorLabel.IsVisible = true;
                return;
            }

            LoginButton.IsEnabled = false;
            
            try
            {
                bool isAuthenticated = await _authService.LoginAsync(Username, Password);
                if (isAuthenticated)
                {
                    // Get the user's role to navigate to the appropriate dashboard
                    string userRole = await _authService.GetCurrentUserRoleAsync();
                    
                    switch (userRole)
                    {
                        case "Administrator":
                            await Shell.Current.GoToAsync("//Administrator/Dashboard");
                            break;
                        case "Operations Manager":
                            await Shell.Current.GoToAsync("//OperationsManager/MainPage");
                            break;
                        case "Environmental Scientist":
                            await Shell.Current.GoToAsync("//EnvironmentalScientist/MainPage");
                            break;
                        default:
                            // If role is unknown, go to the main page
                            await Shell.Current.GoToAsync("//MainPage");
                            break;
                    }
                }
                else
                {
                    ErrorLabel.Text = "Invalid username or password";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"Login error: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
    }
}