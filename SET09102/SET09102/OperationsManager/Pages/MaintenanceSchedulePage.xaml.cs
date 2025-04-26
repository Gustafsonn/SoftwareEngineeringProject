using SET09102.OperationsManager.Models;
using SET09102.OperationsManager.Services;

namespace SET09102.OperationsManager.Pages
{
    public partial class MaintenanceSchedulePage : ContentPage
    {
        private readonly IMaintenanceSchedulingService _maintenanceService;
        private List<MaintenanceSchedule> _allSchedules;

        public MaintenanceSchedulePage(IMaintenanceSchedulingService maintenanceService)
        {
            InitializeComponent();
            _maintenanceService = maintenanceService;
            BindingContext = this;
            LoadMaintenanceSchedules();
        }

        private async void LoadMaintenanceSchedules()
        {
            try
            {
                _allSchedules = (await _maintenanceService.GetAllMaintenanceSchedulesAsync()).ToList();
                MaintenanceCollection.ItemsSource = _allSchedules;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load maintenance schedules: {ex.Message}", "OK");
            }
        }

        private async void OnAddScheduleClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddMaintenanceSchedulePage(_maintenanceService));
        }

        private async void OnViewCalendarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MaintenanceCalendarPage(_maintenanceService));
        }

        private async void OnScheduleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is MaintenanceSchedule selectedSchedule)
            {
                await Navigation.PushAsync(new MaintenanceDetailPage(selectedSchedule, _maintenanceService));
            }
        }
    }
} 