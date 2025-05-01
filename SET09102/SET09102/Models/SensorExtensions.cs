using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET09102.Models
{
    /// <summary>
    /// Extension methods for the Sensor model
    /// </summary>
    public static class SensorExtensions
    {
        /// <summary>
        /// Gets a formatted text representation of when the sensor was last calibrated
        /// </summary>
        /// <param name="sensor">The sensor to get the last calibration text for</param>
        /// <returns>A formatted string showing when the sensor was last calibrated</returns>
        public static string GetLastCalibratedText(this Sensor sensor)
        {
            if (sensor == null)
                return string.Empty;

            var daysSinceCalibration = (DateTime.Now - sensor.LastCalibration).Days;
            
            if (daysSinceCalibration == 0)
                return "Calibrated today";
            else if (daysSinceCalibration == 1)
                return "Calibrated yesterday";
            else if (daysSinceCalibration <= 30)
                return $"Calibrated {daysSinceCalibration} days ago";
            else
                return $"Calibrated on {sensor.LastCalibration:yyyy-MM-dd}";
        }
        
        /// <summary>
        /// Gets a value indicating whether the sensor is due for calibration
        /// </summary>
        /// <param name="sensor">The sensor to check</param>
        /// <returns>True if the sensor is due for calibration, otherwise false</returns>
        public static bool IsDueForCalibration(this Sensor sensor)
        {
            if (sensor == null)
                return false;
                
            return sensor.NextCalibration <= DateTime.Now;
        }
        
        /// <summary>
        /// Gets the color that represents the status of the sensor
        /// </summary>
        /// <param name="sensor">The sensor to get the status color for</param>
        /// <returns>A color name that represents the status</returns>
        public static string GetStatusColor(this Sensor sensor)
        {
            if (sensor == null)
                return "Gray";
                
            if (!sensor.IsActive)
                return "Gray";
                
            return sensor.Status.ToLower() switch
            {
                "operational" => "Green",
                "maintenance" => "Orange",
                "offline" => "Red",
                _ => "Gray"
            };
        }
    }
}