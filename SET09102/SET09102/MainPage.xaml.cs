using Microsoft.Maui.Storage;
using SET09102.Services;
using System.Diagnostics;

namespace SET09102;

public partial class MainPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly DataImportService _dataImportService;

    public MainPage(DatabaseService databaseService, DataImportService dataImportService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _dataImportService = dataImportService;
    }

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Administrator/MainPage");
    }

    private async void OnOpsManagerClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("OperationsManager/MainPage");
    }

    private async void OnEnvScientistClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("EnvironmentalScientist/MainPage");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateDatabaseStatus();
    }

    private void UpdateDatabaseStatus()
    {
        string dbPath = _databaseService.GetDatabasePath();
        if (File.Exists(dbPath))
        {
            DatabaseStatusLabel.Text = "Database initialized";
            DatabaseStatusLabel.TextColor = Colors.Green;
        }
        else
        {
            DatabaseStatusLabel.Text = "Database not initialized";
            DatabaseStatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnInitializeDatabase(object sender, EventArgs e)
    {
        try
        {
            InitializeDatabaseBtn.IsEnabled = false;
            DatabaseStatusLabel.Text = "Initializing database...";
            DatabaseStatusLabel.TextColor = Colors.Orange;

            // Initialize database first
            await _databaseService.InitializeAsync();
            
            // Then import data
            await _dataImportService.ImportSampleDataAsync();

            DatabaseStatusLabel.Text = "Database initialized successfully";
            DatabaseStatusLabel.TextColor = Colors.Green;
        }
        catch (Exception ex)
        {
            DatabaseStatusLabel.Text = "Error initializing database";
            DatabaseStatusLabel.TextColor = Colors.Red;
            await DisplayAlert("Error", $"Failed to initialize database: {ex.Message}", "OK");
        }
        finally
        {
            InitializeDatabaseBtn.IsEnabled = true;
        }
    }

    private async void OnShowDatabaseLocation(object sender, EventArgs e)
    {
        string dbPath = _databaseService.GetDatabasePath();
        string message = $"Database location:\n{dbPath}";
        
        bool answer = await DisplayAlert("Database Location", message, "Open Folder", "OK");
        if (answer)
        {
            try
            {
                // Open the folder containing the database
                string folder = Path.GetDirectoryName(dbPath);
                if (Directory.Exists(folder))
                {
                    Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open folder: {ex.Message}", "OK");
            }
        }
    }
}
