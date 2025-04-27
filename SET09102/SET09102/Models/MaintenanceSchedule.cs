using System;

namespace SET09102.Models
{
    public class MaintenanceSchedule
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTechnician { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
} 