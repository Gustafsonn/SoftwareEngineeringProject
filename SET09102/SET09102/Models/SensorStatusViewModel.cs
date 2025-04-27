using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SET09102.Models;

public class SensorStatusViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<SensorViewModel> _sensors;
    private bool _showIssuesOnly;
    private bool _isLoading;

  public ICommand ToggleIssuesCommand { get; }

public SensorStatusViewModel(DatabaseService databaseService)
{
    _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    _sensors = new ObservableCollection<SensorViewModel>();
    _showIssuesOnly = false;
    _isLoading = false;
    ToggleIssuesCommand = new Command(async () => await ToggleIssuesAsync());
    RefreshCommand = new Command(async () => await LoadSensorsAsync());
    Task.Run(async () => await LoadSensorsAsync());
}

    public ObservableCollection<SensorViewModel> Sensors
    {
        get => _sensors;
        private set
        {
            _sensors = value;
            OnPropertyChanged();
        }
    }

    public bool ShowIssuesOnly
    {
        get => _showIssuesOnly;
        set
        {
            _showIssuesOnly = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    public async Task LoadSensorsAsync()
    {
        IsLoading = true;
        try
        {
            var sensorViewModels = await FetchSensorDataAsync();
            var filteredViewModels = FilterSensors(sensorViewModels);
            Sensors = new ObservableCollection<SensorViewModel>(filteredViewModels);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<List<SensorViewModel>> FetchSensorDataAsync()
    {
        var sensors = await _databaseService.GetSensorsAsync();
        var sensorViewModels = new List<SensorViewModel>();

        foreach (var sensor in sensors)
        {
            if (sensor.Id <= 0)
            {
                continue;
            }

            var alerts = await _databaseService.GetActiveSensorAlertsAsync(sensor.Id) ?? new List<SensorAlert>();
            var logs = await _databaseService.GetRecentMaintenanceLogsAsync(sensor.Id) ?? new List<SensorMaintenanceLog>();
            sensorViewModels.Add(new SensorViewModel
            {
                Sensor = sensor,
                ActiveAlerts = new ObservableCollection<SensorAlert>(alerts),
                RecentMaintenanceLogs = new ObservableCollection<SensorMaintenanceLog>(logs)
            });
        }

        return sensorViewModels;
    }

    private IEnumerable<SensorViewModel> FilterSensors(IEnumerable<SensorViewModel> sensorViewModels)
    {
        var filtered = sensorViewModels.AsEnumerable();
        if (ShowIssuesOnly)
        {
            filtered = sensorViewModels.Where(vm =>
                vm.Sensor.Status != "operational" || vm.ActiveAlerts.Any());
        }
        return filtered.OrderBy(vm => vm.Sensor.Status != "operational" || vm.ActiveAlerts.Any() ? 0 : 1);
    }

    private async Task ToggleIssuesAsync()
    {
        ShowIssuesOnly = !ShowIssuesOnly;
        await LoadSensorsAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SensorViewModel
{
    public Sensor Sensor { get; set; } = null!;
    public ObservableCollection<SensorAlert> ActiveAlerts { get; set; } = new ObservableCollection<SensorAlert>();
    public ObservableCollection<SensorMaintenanceLog> RecentMaintenanceLogs { get; set; } = new ObservableCollection<SensorMaintenanceLog>();
}