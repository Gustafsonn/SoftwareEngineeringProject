using Microsoft.Maui.Controls;
using SET09102.Services;
using System.Diagnostics;

namespace SET09102
{
    public partial class App : Application
    {
        public App(IAuthService authService)
        {
            try
            {
                Debug.WriteLine("App constructor started");
                
                // Do NOT call InitializeComponent
                
                // Pass the auth service to AppShell
                MainPage = new AppShell(authService);
                
                Debug.WriteLine("App constructor completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRASH in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Create a minimal page to show the error
                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "Application Error",
                                FontSize = 24,
                                TextColor = Colors.Red,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = ex.Message,
                                FontSize = 16,
                                TextColor = Colors.Gray,
                                HorizontalOptions = LayoutOptions.Center
                            }
                        }
                    }
                };
            }
        }
    }
}