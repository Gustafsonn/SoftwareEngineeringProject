using System;

namespace SET09102.Models
{
    public class SensorStatusInfo
    {
        public int SensorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastReadingTime { get; set; }
        public double LastReadingValue { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 