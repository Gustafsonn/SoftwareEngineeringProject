namespace SET09102.Models
{
    public class SensorConfiguration
    {
        public int PollingInterval { get; set; }
        public double AlertThreshold { get; set; }
        public string FirmwareVersion { get; set; }
    }
} 