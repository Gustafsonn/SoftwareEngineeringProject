using SET09102.Services;

namespace SET09102.Administrator.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;
        public string Username { get; set; } //= "admin";
        public string Password { get; set; } //= "admin123";

        public LoginPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            BindingContext = this;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorLabel.Text = "Please enter both username and password";
                ErrorLabel.IsVisible = true;
                return;
            }

            bool isAuthenticated = await _authService.LoginAsync(Username, Password);
            if (isAuthenticated)
            {
                await Shell.Current.GoToAsync("//Administrator/MainPage");
            }
            else
            {
                ErrorLabel.Text = "Invalid username or password";
                ErrorLabel.IsVisible = true;
            }
        }
    }
} 