using SET09102.OperationsManager.Models;
using SET09102.OperationsManager.Services;

namespace SET09102.OperationsManager.Pages
{
    public partial class MalfunctionReportPage : ContentPage
    {
        private readonly IMalfunctionReportingService _malfunctionService;
        private List<SensorMalfunction> _allMalfunctions;

        public MalfunctionReportPage(IMalfunctionReportingService malfunctionService)
        {
            InitializeComponent();
            _malfunctionService = malfunctionService;
            BindingContext = this;
            LoadMalfunctions();
        }

        private async void LoadMalfunctions()
        {
            try
            {
                _allMalfunctions = (await _malfunctionService.GetAllMalfunctionReportsAsync()).ToList();
                MalfunctionCollection.ItemsSource = _allMalfunctions;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load malfunction reports: {ex.Message}", "OK");
            }
        }

        private async void OnReportMalfunctionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportMalfunctionPage(_malfunctionService));
        }

        private async void OnMalfunctionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SensorMalfunction selectedMalfunction)
            {
                await Navigation.PushAsync(new MalfunctionDetailPage(selectedMalfunction, _malfunctionService));
            }
        }
    }
} 