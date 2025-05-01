using Microsoft.Maui.Storage;
using SET09102.Services;
using System.Diagnostics;

namespace SET09102
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly DataImportService _dataImportService;
        
        // UI controls we need to reference
        private Label _databaseStatusLabel;
        private Button _initializeDatabaseBtn;
        
        public MainPage(DatabaseService databaseService, DataImportService dataImportService)
        {
            try 
            {
                // Add detailed debug logging
                Debug.WriteLine("MainPage constructor started");
                
                // Store service references
                _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
                _dataImportService = dataImportService ?? throw new ArgumentNullException(nameof(dataImportService));
                
                Debug.WriteLine("Building UI");
                BuildUI();
                
                Debug.WriteLine("MainPage constructor completed");
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during construction
                Debug.WriteLine($"CRASH in MainPage constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Create a basic error UI so at least something shows up
                CreateErrorUI(ex.Message);
                
                // Re-throw to ensure the app knows something went wrong
                throw;
            }
        }
        
        private void BuildUI()
        {
            // Create UI elements
            _databaseStatusLabel = new Label
            {
                Text = "Database not initialized",
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center
            };

            _initializeDatabaseBtn = new Button
            {
                Text = "Initialize Database",
                HorizontalOptions = LayoutOptions.Center
            };
            _initializeDatabaseBtn.Clicked += OnInitializeDatabase;

            var showDatabaseBtn = new Button
            {
                Text = "Show Database Location",
                HorizontalOptions = LayoutOptions.Center
            };
            showDatabaseBtn.Clicked += OnShowDatabaseLocation;

            var adminBtn = new Button
            {
                Text = "Administrator",
                WidthRequest = 120
            };
            adminBtn.Clicked += OnAdminClicked;

            var opsManagerBtn = new Button
            {
                Text = "Operations Manager",
                WidthRequest = 120
            };
            opsManagerBtn.Clicked += OnOpsManagerClicked;

            var envScientistBtn = new Button
            {
                Text = "Environmental Scientist",
                WidthRequest = 120
            };
            envScientistBtn.Clicked += OnEnvScientistClicked;

            var titleLabel = new Label
            {
                Text = "Environmental Monitoring",
                FontSize = 32,
                HorizontalOptions = LayoutOptions.Center
            };

            var dbStatusTitleLabel = new Label
            {
                Text = "Database Status",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center
            };

            var btnStack = new HorizontalStackLayout
            {
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                Children = { adminBtn, opsManagerBtn, envScientistBtn }
            };

            // Main layout
            var mainLayout = new VerticalStackLayout
            {
                Spacing = 25,
                Padding = new Thickness(30, 0),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    titleLabel,
                    btnStack,
                    dbStatusTitleLabel,
                    _databaseStatusLabel,
                    _initializeDatabaseBtn,
                    showDatabaseBtn
                }
            };

            // Create a ScrollView to wrap the main layout
            var scrollView = new ScrollView
            {
                Content = mainLayout
            };

            // Set as page content
            Content = scrollView;
        }
        
        private void CreateErrorUI(string errorMessage)
        {
            // Create a simple error UI in case the main UI fails to build
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "An error occurred",
                        FontSize = 24,
                        TextColor = Colors.Red,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = errorMessage,
                        FontSize = 16,
                        TextColor = Colors.Gray,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
            };
        }

        private async void OnAdminClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//Administrator/Login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Unable to navigate to Administrator: {ex.Message}", "OK");
            }
        }

        private async void OnOpsManagerClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//OperationsManager/MainPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Unable to navigate to Operations Manager: {ex.Message}", "OK");
            }
        }

        private async void OnEnvScientistClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//EnvironmentalScientist/MainPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Unable to navigate to Environmental Scientist: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                Debug.WriteLine("MainPage.OnAppearing called");
                base.OnAppearing();
                UpdateDatabaseStatus();
                Debug.WriteLine("MainPage.OnAppearing completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        private void UpdateDatabaseStatus()
        {
            try
            {
                string dbPath = _databaseService.GetDatabasePath();
                if (File.Exists(dbPath))
                {
                    _databaseStatusLabel.Text = "Database initialized";
                    _databaseStatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    _databaseStatusLabel.Text = "Database not initialized";
                    _databaseStatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating database status: {ex.Message}");
                _databaseStatusLabel.Text = "Error checking database";
                _databaseStatusLabel.TextColor = Colors.Red;
            }
        }

        private async void OnInitializeDatabase(object sender, EventArgs e)
        {
            try
            {
                _initializeDatabaseBtn.IsEnabled = false;
                _databaseStatusLabel.Text = "Initializing database...";
                _databaseStatusLabel.TextColor = Colors.Orange;

                // Initialize database first
                await _databaseService.InitializeAsync();
                
                // Then import data
                await _dataImportService.ImportSampleDataAsync();

                _databaseStatusLabel.Text = "Database initialized successfully";
                _databaseStatusLabel.TextColor = Colors.Green;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Database initialization error: {ex.Message}");
                _databaseStatusLabel.Text = "Error initializing database";
                _databaseStatusLabel.TextColor = Colors.Red;
                await DisplayAlert("Error", $"Failed to initialize database: {ex.Message}", "OK");
            }
            finally
            {
                _initializeDatabaseBtn.IsEnabled = true;
            }
        }

        private async void OnShowDatabaseLocation(object sender, EventArgs e)
        {
            try
            {
                string dbPath = _databaseService.GetDatabasePath();
                string message = $"Database location:\n{dbPath}";
                
                bool answer = await DisplayAlert("Database Location", message, "Open Folder", "OK");
                if (answer)
                {
                    try
                    {
                        // Open the folder containing the database
                        string? folder = Path.GetDirectoryName(dbPath);
                        if (folder != null && Directory.Exists(folder))
                        {
                            // This will only work on Windows
                            Process.Start("explorer.exe", folder);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error opening folder: {ex.Message}");
                        await DisplayAlert("Error", $"Could not open folder: {ex.Message}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing database location: {ex.Message}");
                await DisplayAlert("Error", $"Failed to get database location: {ex.Message}", "OK");
            }
        }
    }
}