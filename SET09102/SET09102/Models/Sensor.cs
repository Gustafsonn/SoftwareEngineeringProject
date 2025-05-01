using System.ComponentModel;

namespace SET09102.Models;

public class Sensor : INotifyPropertyChanged
{
    private string _location;
    private string _firmwareVersion;

    public int Id { get; set; }
    public int SiteId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Unit { get; set; }
    public string Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }
    }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SiteType { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Agglomeration { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastCalibration { get; set; }
    public DateTime NextCalibration { get; set; }
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set
        {
            if (_firmwareVersion != value)
            {
                _firmwareVersion = value;
                OnPropertyChanged(nameof(FirmwareVersion));
            }
        }
    }
    public DateTime? LastMaintenance { get; set; }
    public DateTime? NextMaintenance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Extension properties for UI display
    public string StatusColor => this.GetStatusColor();
    public string LastCalibratedText => this.GetLastCalibratedText();
    public bool IsDueForCalibration => this.IsDueForCalibration();
    
    // Get a status text that properly formats the status with uppercase first letter
    public string StatusText => string.IsNullOrEmpty(Status) 
        ? "Unknown" 
        : char.ToUpper(Status[0]) + Status.Substring(1).ToLower();
}
