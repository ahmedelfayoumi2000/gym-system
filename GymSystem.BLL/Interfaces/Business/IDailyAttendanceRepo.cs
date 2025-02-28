using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;


namespace GymSystem.BLL.Interfaces
{
    public interface IDailyAttendanceRepo
    {
        Task<ApiResponse> AddAttendanceAsync(DailyAttendanceDto attendance);
        Task<IReadOnlyList<DailyAttendanceDto>> GetAttendancesForUserAsync(string userCode);
        Task<ApiResponse> DeleteAttendanceAsync(int id);
    }
}