namespace SET09102.Models;

public class SensorAlert
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public Sensor Sensor { get; set; } = new();
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double ThresholdValue { get; set;  }
    public double MeasuredValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}