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
            builder.Services.AddSingleton<SensorService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IPreferences>(Preferences.Default);
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<SensorSettingsService>();

            // Register pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}