using SET09102.Models;
using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SET09102.OperationsManager.Pages;

public partial class MalfunctionsPage : ContentPage, INotifyPropertyChanged
{
	private readonly ISensorService _sensorService;
    private ObservableCollection<Sensor> _sensors = [];
    private ObservableCollection<Malfunction> _malfunctions = [];
    private Sensor? _selectedSensor = null;    
    public ICommand AddNewMalfunctionCommand { get; }
    public ICommand ResolveMalfunctionCommand { get;  }

    public MalfunctionsPage(ISensorService sensorService)
	{
		_sensorService = sensorService;
        AddNewMalfunctionCommand = new Command(OnAddNewMalfunction);
        ResolveMalfunctionCommand = new Command<int>(OnResolveMalfunction);
        InitializeComponent();
        BindingContext = this;
        _ = LoadSensorsAsync();
	}

    public ObservableCollection<Sensor> Sensors
    {
        get => _sensors;
        set
        {
            _sensors = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Malfunction> Malfunctions
    {
        get => _malfunctions;
        set
        {
            _malfunctions = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SensorMalfunctionSummary));
        }
    }

    public Sensor? SelectedSensor
    {
        get => _selectedSensor;
        set
        {
            if (_selectedSensor != value)
            {
                _selectedSensor = value;                
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSensorSelected));
                OnPropertyChanged(nameof(NoSensorSelected));
                _ = LoadMalfunctionsAsync();
            }
        }
    }

    public bool IsSensorSelected => SelectedSensor != null;

    public bool NoSensorSelected => SelectedSensor == null;

    public string SensorMalfunctionSummary => 
        $"{SelectedSensor?.Name ?? "Unknown"} - {Malfunctions?.Count ?? 0} malfunction(s)";

    
    public async void OnAddNewMalfunction()
    {        
        try
        {
            var description = await DisplayPromptAsync(
                "New Malfunction",
                "Enter description for the malfunction",                
                maxLength: 500
            );

            if (string.IsNullOrWhiteSpace(description)) return;

            var malfunction = new Malfunction
            {
                SensorId = SelectedSensor?.Id ?? throw new InvalidOperationException("No sensor selected."),
                Description = description,
                Resolved = false
            };

            await _sensorService.CreateMalfunctionAsync(malfunction);
            await LoadMalfunctionsAsync();            
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                $"Error creating a new sensor malfunction - {ex.Message}",
                "OK");
        }
                
    }

    public async void OnResolveMalfunction(int malfunctionId)
    {
        try
        {
            var malfunction = Malfunctions.SingleOrDefault(item => item.Id == malfunctionId);

            if (malfunction == null || malfunction.Resolved) return;
            var updatedMalfunction = malfunction.Clone();
            updatedMalfunction.Resolved = true;
            await _sensorService.UpdateMalfunctionAsync(updatedMalfunction);
            malfunction.Resolved = true;
        }
        catch(Exception ex)
        {
            await DisplayAlert(
                "Error",
                $"Error marking sensor malfunction as resolved - {ex.Message}",
                "OK");

        }
    }

    private async Task LoadSensorsAsync()
    {
        Sensors = await _sensorService.GetSensorsAsync();
    }

    private async Task LoadMalfunctionsAsync()
    {
        if (SelectedSensor == null)
        {
            Malfunctions = [];
            return;
        }

        Malfunctions = await _sensorService.GetMalfunctionsAsync(SelectedSensor.Id);
    }
}