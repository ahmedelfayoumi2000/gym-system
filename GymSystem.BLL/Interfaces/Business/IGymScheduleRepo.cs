using GymSystem.BLL.Dtos.GymSchedule;
using GymSystem.BLL.Errors;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IGymScheduleRepo
    {
        Task<ApiResponse> AddSchedule(GymScheduleDto scheduleDto);
        Task<ApiResponse> UpdateSchedule(int id, GymScheduleDto scheduleDto);
        Task<ApiResponse> DeleteSchedule(int id);
        Task<ApiResponse> GetSchedule(int id);
        Task<ApiResponse> GetSchedules();
        Task<ApiResponse> GetSchedulesByDay(string dayOfWeek);
    }
}
