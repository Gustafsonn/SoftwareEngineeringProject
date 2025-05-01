using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Models;

public class Sensor : INotifyPropertyChanged
{
    private int _id;
    private int _siteId;
    private string _name = string.Empty;
    private string _type = string.Empty;
    private string _unit = string.Empty;
    private string _location = string.Empty;
    private double _latitude;
    private double _longitude;
    private string _siteType = string.Empty;
    private string _zone = string.Empty;
    private string _agglomeration = string.Empty;
    private string _authority = string.Empty;
    private bool _isActive;
    private DateTime _lastCalibration;
    private DateTime _nextCalibration;
    private double? _minThreshold;
    private double? _maxThreshold;
    private string _firmwareVersion = string.Empty;
    private DateTime? _lastMaintenance;
    private DateTime? _nextMaintenance;
    private string _status = string.Empty;
    private string _description = string.Empty;
    private DateTime _createdAt;
    private DateTime _updatedAt;

    public int Id 
    { 
        get => _id; 
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }
    
    public int SiteId 
    { 
        get => _siteId; 
        set
        {
            if (_siteId != value)
            {
                _siteId = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Name 
    { 
        get => _name; 
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Type 
    { 
        get => _type; 
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Unit 
    { 
        get => _unit; 
        set
        {
            if (_unit != value)
            {
                _unit = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Location 
    { 
        get => _location; 
        set
        {
            if (_location != value)
            {
                _location = value;
                OnPropertyChanged();
            }
        }
    }
    
    public double Latitude 
    { 
        get => _latitude; 
        set
        {
            if (_latitude != value)
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }
    }
    
    public double Longitude 
    { 
        get => _longitude; 
        set
        {
            if (_longitude != value)
            {
                _longitude = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string SiteType 
    { 
        get => _siteType; 
        set
        {
            if (_siteType != value)
            {
                _siteType = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Zone 
    { 
        get => _zone; 
        set
        {
            if (_zone != value)
            {
                _zone = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Agglomeration 
    { 
        get => _agglomeration; 
        set
        {
            if (_agglomeration != value)
            {
                _agglomeration = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Authority 
    { 
        get => _authority; 
        set
        {
            if (_authority != value)
            {
                _authority = value;
                OnPropertyChanged();
            }
        }
    }
    
    public bool IsActive 
    { 
        get => _isActive; 
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }
    
    public DateTime LastCalibration 
    { 
        get => _lastCalibration; 
        set
        {
            if (_lastCalibration != value)
            {
                _lastCalibration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastCalibratedText));
                OnPropertyChanged(nameof(IsDueForCalibration));
            }
        }
    }
    
    public DateTime NextCalibration 
    { 
        get => _nextCalibration; 
        set
        {
            if (_nextCalibration != value)
            {
                _nextCalibration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDueForCalibration));
            }
        }
    }
    
    public double? MinThreshold 
    { 
        get => _minThreshold; 
        set
        {
            if (_minThreshold != value)
            {
                _minThreshold = value;
                OnPropertyChanged();
            }
        }
    }
    
    public double? MaxThreshold 
    { 
        get => _maxThreshold; 
        set
        {
            if (_maxThreshold != value)
            {
                _maxThreshold = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string FirmwareVersion 
    { 
        get => _firmwareVersion; 
        set
        {
            if (_firmwareVersion != value)
            {
                _firmwareVersion = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime? LastMaintenance 
    { 
        get => _lastMaintenance; 
        set
        {
            if (_lastMaintenance != value)
            {
                _lastMaintenance = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime? NextMaintenance 
    { 
        get => _nextMaintenance; 
        set
        {
            if (_nextMaintenance != value)
            {
                _nextMaintenance = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string Status 
    { 
        get => _status; 
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }
    
    public string Description 
    { 
        get => _description; 
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime CreatedAt 
    { 
        get => _createdAt; 
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                OnPropertyChanged();
            }
        }
    }
    
    public DateTime UpdatedAt 
    { 
        get => _updatedAt; 
        set
        {
            if (_updatedAt != value)
            {
                _updatedAt = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Extension properties for UI display
    public string StatusColor => this.GetStatusColor();
    public string LastCalibratedText => this.GetLastCalibratedText();
    public bool IsDueForCalibration => this.IsDueForCalibration();
    
    // Get a status text that properly formats the status with uppercase first letter
    public string StatusText => string.IsNullOrEmpty(Status) 
        ? "Unknown" 
        : char.ToUpper(Status[0]) + Status.Substring(1).ToLower();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}