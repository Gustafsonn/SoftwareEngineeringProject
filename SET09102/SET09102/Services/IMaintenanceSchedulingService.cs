using System.Collections.Generic;
using System.Threading.Tasks;
using SET09102.Models;

namespace SET09102.Services
{
    public interface IMaintenanceSchedulingService
    {
        Task<IEnumerable<MaintenanceSchedule>> GetMaintenanceSchedulesAsync();
        Task<MaintenanceSchedule> GetMaintenanceScheduleAsync(int sensorId);
        Task<bool> ScheduleMaintenanceAsync(MaintenanceSchedule schedule);
        Task<bool> UpdateMaintenanceScheduleAsync(MaintenanceSchedule schedule);
        Task<bool> CancelMaintenanceScheduleAsync(int scheduleId);
    }
} 