using System;

namespace SET09102.Services
{
    public interface ILogger
    {
        void LogError(string message);
        void LogInformation(string message);
    }
    
    public class Logger : ILogger
    {
        public void LogError(string message)
        {
            // In a real implementation, this would log to a file or database
            Console.WriteLine($"ERROR: {message}");
        }
        
        public void LogInformation(string message)
        {
            // In a real implementation, this would log to a file or database
            Console.WriteLine($"INFO: {message}");
        }
    }
}