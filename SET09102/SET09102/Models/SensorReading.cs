using System;

namespace SET09102.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public DateTime ReadingTime { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsAnomaly { get; set; }
    }
} 