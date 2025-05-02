using SET09102.Services;
using Microsoft.Maui.Storage;
using SET09102.Administrator.Pages;

namespace SET09102
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ISensorService>(_ => new SensorService(new DatabaseService().GetDatabasePath()));
            builder.Services.AddSingleton<DataImportService>();            
            builder.Services.AddSingleton<SensorSettingsService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddTransient<MainPage>(); // Make sure MainPage is registered
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SensorMonitoringPage>();
            builder.Services.AddTransient<DataStoragePage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddSingleton<AppShell>();

            // Create a logger for debugging
            builder.Services.AddLogging();

            return builder.Build();
        }
    }
}