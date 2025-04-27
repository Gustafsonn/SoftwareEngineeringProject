using Microsoft.Maui.Controls;

namespace SET09102.EnvironmentalScientist.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnHistoricalDataClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//EnvironmentalScientist/HistoricalData");
    }

    private async void OnMapViewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//EnvironmentalScientist/MapPage");
    }
}