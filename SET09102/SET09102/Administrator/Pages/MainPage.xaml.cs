using Microsoft.Maui.Controls;

namespace SET09102.Administrator.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        // Already on dashboard, do nothing
    }

    private async void OnMapViewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Administrator/MapPage");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Administrator/SettingsPage");
    }
} 