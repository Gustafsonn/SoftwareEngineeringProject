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
        InitializeComponent();
        _databaseService = databaseService;
        _dataImportService = dataImportService;
        _shell = shell;

        // Initialize SQLite
        SQLitePCL.Batteries_V2.Init();

        MainPage = _shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }
}