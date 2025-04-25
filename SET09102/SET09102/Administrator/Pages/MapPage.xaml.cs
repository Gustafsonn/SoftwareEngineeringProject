using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SET09102.Services;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;

namespace SET09102.Administrator.Pages
{
    public partial class MapPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private bool _isInitialized;

        public MapPage()
        {
            try
            {
                InitializeComponent();
                _databaseService = new DatabaseService();
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MapPage constructor: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (!_isInitialized)
            {
                try
                {
                    await InitializeMapAsync();
                    _isInitialized = true;
                }
                catch (COMException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"COM Exception in OnAppearing: {ex.Message}");
                    await DisplayAlert("Map Error", 
                        "Failed to initialize the map control. Please ensure you have the necessary permissions and try again.", 
                        "OK");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    await DisplayAlert("Error", 
                        $"Failed to initialize map: {ex.Message}", 
                        "OK");
                }
            }
        }

        private async Task InitializeMapAsync()
        {
            try
            {
                // Set default location to Edinburgh
                var defaultLocation = new Location(55.9533, -3.1883); // Edinburgh coordinates
                var mapSpan = MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(1));
                
                // Ensure we're on the main thread when updating the map
                if (MainThread.IsMainThread)
                {
                    SensorMap.MoveToRegion(mapSpan);
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() => SensorMap.MoveToRegion(mapSpan));
                }

                // Check if database exists and is initialized
                string dbPath = _databaseService.GetDatabasePath();
                if (!File.Exists(dbPath))
                {
                    await DisplayAlert("Database Not Initialized", 
                        "Please initialize the database from the main page before viewing the map.", 
                        "OK");
                    return;
                }

                // Verify database connection and schema
                try
                {
                    using var connection = new SqliteConnection($"Data Source={dbPath}");
                    await connection.OpenAsync();

                    // Check if sensors table exists
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='sensors'";
                    var result = await command.ExecuteScalarAsync();
                    
                    if (result == null)
                    {
                        await DisplayAlert("Database Error", 
                            "The database schema is not properly initialized. Please reinitialize the database from the main page.", 
                            "OK");
                        return;
                    }
                }
                catch (SqliteException ex)
                {
                    await DisplayAlert("Database Error", 
                        $"Failed to verify database: {ex.Message}", 
                        "OK");
                    return;
                }

                // Load sensors
                await LoadSensorsAsync();
            }
            catch (COMException ex)
            {
                System.Diagnostics.Debug.WriteLine($"COM Exception in InitializeMapAsync: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeMapAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private async Task LoadSensorsAsync()
        {
            try
            {
                using var connection = _databaseService.GetConnection();
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, latitude, longitude, type FROM sensors";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var sensorId = reader.GetInt32(0);
                    var latitude = reader.GetDouble(1);
                    var longitude = reader.GetDouble(2);
                    var sensorType = reader.GetString(3);

                    var pin = new Pin
                    {
                        Label = $"Sensor {sensorId}",
                        Address = sensorType,
                        Location = new Location(latitude, longitude),
                        Type = PinType.Place
                    };

                    // Ensure we're on the main thread when adding pins
                    if (MainThread.IsMainThread)
                    {
                        SensorMap.Pins.Add(pin);
                    }
                    else
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => SensorMap.Pins.Add(pin));
                    }
                }
            }
            catch (SqliteException ex)
            {
                await DisplayAlert("Database Error", $"Failed to load sensors: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/MainPage");
        }

        private async void OnMapViewClicked(object sender, EventArgs e)
        {
            // Already on map view
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Administrator/SettingsPage");
        }
    }
} 