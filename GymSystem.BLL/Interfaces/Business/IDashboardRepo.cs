using GymSystem.BLL.Dtos.Dashboard;
using GymSystem.BLL.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IDashboardRepo
    {
        Task<ApiResponse> GetDashboardStats(string userId, int year, int month);
        Task<ApiResponse> AddDailyStats(string userId, AddDailyStatsDto statsDto);
    }
}
