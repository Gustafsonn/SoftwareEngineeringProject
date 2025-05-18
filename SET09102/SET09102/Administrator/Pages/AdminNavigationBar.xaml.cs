namespace SET09102.Administrator.Pages
{
    /// <summary>
    /// A reusable navigation bar component for the Administrator section of the application
    /// that provides navigation to key admin pages.
    /// </summary>
    /// <remarks>
    /// This class handles Shell-based navigation for the Administrator section.
    /// Each button click triggers navigation to a different page in the admin module.
    /// The navigation uses Shell's URI-based navigation pattern with relative paths.
    /// </remarks>
    public partial class AdminNavigationBar : ContentView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNavigationBar"/> class.
        /// </summary>
        public AdminNavigationBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Navigates to the main application dashboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        /// <summary>
        /// Navigates to the Administrator home page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/MainPage");
        }
        
        /// <summary>
        /// Navigates to the Sensor Configuration page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnSensorConfigClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SensorConfigurationPage");
        }

        /// <summary>
        /// Navigates to the Data Storage page for database management.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnDataClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/DataStoragePage");
        }
        
        /// <summary>
        /// Navigates to the Sensor Monitoring page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnSensorMonitorClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SensorMonitoringPage");
        }

        /// <summary>
        /// Navigates to the Settings page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SettingsPage");
        }
    }
}