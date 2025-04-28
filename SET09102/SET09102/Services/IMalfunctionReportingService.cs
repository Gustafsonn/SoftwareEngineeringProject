using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET09102.Services
{
    public interface IMalfunctionReportingService
    {
        Task GetAllMalfunctionReportsAsync();
    }
}
