namespace SET09102.OperationsManager.Pages;

public partial class OpsManagerNavigationBar : ContentView
{
    public OpsManagerNavigationBar()
    {
        InitializeComponent();
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//OperationsManager/MainPage");
    }

    private async void OnDataVerificationClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//OperationsManager/DataVerificationPage");
    }
}
