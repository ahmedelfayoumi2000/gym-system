using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;


namespace GymSystem.BLL.Interfaces
{
    public interface IDailyAttendanceRepo
    {
        Task<ApiResponse> AddAttendanceAsync(DailyAttendanceDto attendance);
        Task<IReadOnlyList<DailyAttendanceDto>> GetAttendancesForUserAsync(string userCode);
        Task<ApiResponse> DeleteAttendanceAsync(int id);


        Task<QRCodeDto> GenerateQRCodeAsync(string userId);
        Task<ApiResponse> CheckInAsync(AttendanceCheckInDto checkInDto, string currentUserId);
    }
}