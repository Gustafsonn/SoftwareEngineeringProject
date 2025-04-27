using SET09102.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SET09102.Models;

public class HistoricalDataViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private ObservableCollection<EnvironmentalDataEntity> _historicalData;
    private ObservableCollection<string> _dataTypes;
    private string _selectedDataType;

    public HistoricalDataViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _historicalData = new ObservableCollection<EnvironmentalDataEntity>();
        _dataTypes = new ObservableCollection<string>
        {
            "Temperature",
            "Humidity",
            "AirQuality"
        };
        _selectedDataType = string.Empty;
    }

    public ObservableCollection<EnvironmentalDataEntity> HistoricalData
    {
        get => _historicalData;
        set
        {
            _historicalData = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> DataTypes
    {
        get => _dataTypes;
        set
        {
            _dataTypes = value;
            OnPropertyChanged();
        }
    }

    public string SelectedDataType
    {
        get => _selectedDataType;
        set
        {
            _selectedDataType = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadDataAsync()
    {
        var data = await _databaseService.GetEnvironmentalDataAsync();
        if (!string.IsNullOrEmpty(SelectedDataType))
        {
            data = data.Where(d => d.DataType == SelectedDataType).ToList();
        }
        HistoricalData = new ObservableCollection<EnvironmentalDataEntity>(data);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}