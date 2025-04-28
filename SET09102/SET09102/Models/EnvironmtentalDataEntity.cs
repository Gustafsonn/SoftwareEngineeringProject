namespace SET09102.Models
{
    // Entity class for environmental data
    public class EnvironmentalDataEntity
    {
        // Initialize properties with default values to avoid null reference issues
        public string DataType { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }
}