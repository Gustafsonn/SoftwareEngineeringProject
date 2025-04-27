using SET09102.Services;
using Microsoft.Data.Sqlite;

namespace SET09102;

public partial class App : Application
{
    private readonly DatabaseService _databaseService;
    private readonly DataImportService _dataImportService;
    private readonly AppShell _shell;

    public App(DatabaseService databaseService, DataImportService dataImportService, AppShell shell)
    {
        try 
        {
            InitializeComponent();

            _databaseService = databaseService;
            _dataImportService = dataImportService;
            _shell = shell;

            // Set the main page directly to avoid startup issues
            MainPage = _shell;

            // Initialize async work after UI is ready
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                try 
                {
                    System.Diagnostics.Debug.WriteLine("Starting database initialization...");
                    await _databaseService.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("Database initialized successfully");
                    
                    System.Diagnostics.Debug.WriteLine("Starting data import...");
                    await _dataImportService.ImportSampleDataAsync();
                    System.Diagnostics.Debug.WriteLine("Data import completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: {ex}");
                    
                    // Only try to show an alert if we have a valid main page
                    if (Current?.MainPage != null)
                    {
                        await Current.MainPage.DisplayAlert("Initialization Error", 
                            "The application could not initialize properly. Check logs for details.", 
                            "OK");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in App constructor: {ex}");
        }
    }
}