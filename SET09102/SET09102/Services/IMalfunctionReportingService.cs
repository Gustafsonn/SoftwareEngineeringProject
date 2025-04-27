using System.Collections.Generic;
using System.Threading.Tasks;
using SET09102.Models;

namespace SET09102.Services
{
    public interface IMalfunctionReportingService
    {
        Task<IEnumerable<SensorMalfunction>> GetMalfunctionsAsync();
        Task<SensorMalfunction> GetMalfunctionAsync(int malfunctionId);
        Task<bool> ReportMalfunctionAsync(SensorMalfunction malfunction);
        Task<bool> UpdateMalfunctionStatusAsync(int malfunctionId, string status);
        Task<bool> ResolveMalfunctionAsync(int malfunctionId, string resolutionNotes);
    }
} 