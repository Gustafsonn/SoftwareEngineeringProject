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
    public string SiteType { get; set; }
    public string Zone { get; set; }
    public string Agglomeration { get; set; }
    public string Authority { get; set; }
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
    public string Status { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 