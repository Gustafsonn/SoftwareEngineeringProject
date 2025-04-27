namespace SET09102.Models
{
    public class EnvironmentalDataPoint
    {
        public DateTime Timestamp { get; set; }
        public string Location { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string SensorName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}