using System;

namespace SET09102.Models
{
    public class SensorMalfunction
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public DateTime ReportDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;
        public DateTime? ResolutionDate { get; set; }
    }
} 