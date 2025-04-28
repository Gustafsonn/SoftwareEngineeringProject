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
            builder.Services.AddSingleton<DataImportService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<ISensorService>(_ => new SensorService(new DatabaseService().GetDatabasePath()));
                            
            // Register pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
