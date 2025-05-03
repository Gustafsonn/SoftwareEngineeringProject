using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SET09102.Models;

public class Malfunction : INotifyPropertyChanged
{
    private int _id;
    private int _sensorId;
    private string _description = string.Empty;
    private bool _resolved;

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

    public int SensorId
    {
        get => _sensorId;
        set
        {
            if (_sensorId != value)
            {
                _sensorId = value;
                OnPropertyChanged();
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

    public bool Resolved
    {
        get => _resolved;
        set
        {
            if (_resolved != value)
            {
                _resolved = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Active));
            }
        }
    }

    public Malfunction Clone()
    {
        return new()
        {
            Id = Id,
            SensorId = SensorId,
            Description = Description,
            Resolved = Resolved
        };
    }

    public bool Active
    {
        get => !Resolved;
        set => Resolved = !value;       
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
