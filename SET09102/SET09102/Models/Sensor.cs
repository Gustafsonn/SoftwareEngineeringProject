using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Models;

/// <summary>
/// Represents an environmental monitoring sensor with its properties, status, and metadata.
/// Implements INotifyPropertyChanged to support UI data binding.
/// </summary>
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

    /// <summary>
    /// Gets or sets the unique identifier for the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the site identifier where the sensor is located.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the descriptive name of the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the type of sensor (e.g., "Air Quality", "Water Quality", "Weather").
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the measurement unit of the sensor (e.g., "°C", "µg/m³").
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the physical location description of the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the latitude coordinate of the sensor's location.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the longitude coordinate of the sensor's location.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the site type where the sensor is located (e.g., "Urban Traffic").
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the geographical zone of the sensor's location.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the urban agglomeration of the sensor's location.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the administrative authority responsible for the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets a value indicating whether the sensor is currently active.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the date and time of the sensor's last calibration.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the scheduled date and time for the sensor's next calibration.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the minimum threshold value for the sensor's measurements.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the maximum threshold value for the sensor's measurements.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the current firmware version of the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the date and time of the sensor's last maintenance.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the scheduled date and time for the sensor's next maintenance.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the current operational status of the sensor (e.g., "operational", "maintenance", "offline").
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets a detailed description of the sensor.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the date and time when the sensor record was created.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the date and time when the sensor record was last updated.
    /// </summary>
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

    /// <summary>
    /// Gets a color representing the current status of the sensor.
    /// </summary>
    public string StatusColor => this.GetStatusColor();
    
    /// <summary>
    /// Gets a formatted text representation of when the sensor was last calibrated.
    /// </summary>
    public string LastCalibratedText => this.GetLastCalibratedText();
    
    /// <summary>
    /// Gets a value indicating whether the sensor is due for calibration.
    /// </summary>
    public bool IsDueForCalibration => this.IsDueForCalibration();
    
    /// <summary>
    /// Gets a properly formatted status text with the first letter capitalized.
    /// </summary>
    public string StatusText => string.IsNullOrEmpty(Status) 
        ? "Unknown" 
        : char.ToUpper(Status[0]) + Status.Substring(1).ToLower();

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Creates a new Sensor instance with the same property values as this instance.
    /// </summary>
    /// <returns>A new Sensor instance with copied property values.</returns>
    public Sensor Clone()
    {
        return new()
        {
            Id = Id,
            SiteId = SiteId,
            Name = Name,
            Type = Type,
            Unit = Unit,
            Location = Location,
            Latitude = Latitude,
            Longitude = Longitude,
            SiteType = SiteType,
            Zone = Zone,
            Agglomeration = Agglomeration,
            Authority = Authority,
            IsActive = IsActive,
            LastCalibration = LastCalibration,
            NextCalibration = NextCalibration,
            MinThreshold = MinThreshold,
            MaxThreshold = MaxThreshold,
            FirmwareVersion = FirmwareVersion,
            LastMaintenance = LastMaintenance,
            NextMaintenance = NextMaintenance,
            Status = Status,
            Description = Description,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }

}