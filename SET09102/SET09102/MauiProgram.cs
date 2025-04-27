using SET09102.Services;
using Microsoft.Maui.Storage;
using SET09102.Administrator.Pages;
using SET09102.Models;
using SET09102.OperationsManager.Pages;
using CommunityToolkit.Maui; 
namespace SET09102;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<DataImportService>();
        builder.Services.AddSingleton<SensorService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

        // Register ViewModels
        builder.Services.AddSingleton<SensorStatusViewModel>();

        // Register pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<SensorStatusPage>();

        return builder.Build();
    }
}