using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IDailyAttendanceRepo
    {
        Task<ApiResponse> AddAttendanceAsync(AttendanceDto attendanceDto);
        Task<IReadOnlyList<AttendanceDto>> GetAttendancesForUserAsync(string userCode);
        Task<int> GetCountAsync(ISpecification<Attendance> spec);
        Task<IReadOnlyList<Attendance>> GetAllWithSpecAsync(ISpecification<Attendance> spec);
        Task<ApiResponse> DeleteAttendanceAsync(int id);
        Task<QRCodeDto> GenerateQRCodeAsync(string userId);
        Task<ApiResponse> CheckInAsync(AttendanceCheckInDto checkInDto, string currentUserId);
    }
}