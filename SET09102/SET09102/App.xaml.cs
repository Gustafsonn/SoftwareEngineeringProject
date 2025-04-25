using SET09102.Services;
using Microsoft.Data.Sqlite;

namespace SET09102;

public partial class App : Application
{
    private readonly DatabaseService _databaseService;
    private readonly DataImportService _dataImportService;
    private AppShell? _shell;

    public App(DatabaseService databaseService, DataImportService dataImportService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _dataImportService = dataImportService;

        // Initialize SQLite
        SQLitePCL.Batteries_V2.Init();

        _shell = new AppShell();
        MainPage = _shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell ?? new AppShell());
    }
}