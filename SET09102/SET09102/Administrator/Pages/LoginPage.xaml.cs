using SET09102.Services;

namespace SET09102.Administrator.Pages
{
    /// <summary>
    /// Represents the login page for the Administrator section of the application.
    /// Allows users to enter credentials and attempt authentication.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Gets or sets the username entered by the user.
        /// </summary>
        /// <remarks>This property is bound to the username input field in the XAML.</remarks>
        public string Username { get; set; } //= "admin";

        /// <summary>
        /// Gets or sets the password entered by the user.
        /// </summary>
        /// <remarks>This property is bound to the password input field in the XAML.</remarks>
        public string Password { get; set; } //= "admin123";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        /// <param name="authService">The authentication service used to validate user credentials.</param>
        public LoginPage(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService; // Store the injected authentication service
            BindingContext = this; // Set the binding context for data binding in XAML
        }

        /// <summary>
        /// Handles the event when the login button is clicked.
        /// Validates input, attempts authentication, and navigates on success or displays an error on failure.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Basic input validation
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorLabel.Text = "Please enter both username and password";
                ErrorLabel.IsVisible = true;
                return; // Stop execution if validation fails
            }

            // Attempt to authenticate using the provided credentials
            bool isAuthenticated = await _authService.LoginAsync(Username, Password);
            
            if (isAuthenticated)
            {
                // Clear any previous error messages
                ErrorLabel.IsVisible = false; 
                // Navigate to the main administrator page upon successful login
                await Shell.Current.GoToAsync("//Administrator/MainPage");
            }
            else
            {
                // Display an error message for invalid credentials
                ErrorLabel.Text = "Invalid username or password";
                ErrorLabel.IsVisible = true;
            }
        }
    }
}