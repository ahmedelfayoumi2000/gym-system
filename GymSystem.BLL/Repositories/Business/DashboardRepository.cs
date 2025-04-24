using AutoMapper;
using GymSystem.BLL.Dtos.Dashboard;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications.AttendanceSpec;
using GymSystem.BLL.Specifications.UserStatsSpec;
using GymSystem.DAL.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class DashboardRepository : IDashboardRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse> GetDashboardStats(string userId, int year, int month)
        {
            try
            {
                // جلب أيام الحضور في الشهر المحدد
                var attendanceSpec = new UserAttendanceByMonthSpecification(userId, year, month);
                var attendance = await _unitOfWork.Repository<Attendance>().GetAllWithSpecAsync(attendanceSpec);
                var attendanceDates = attendance.Select(a => a.AttendanceDate).ToList();

                // جلب الإحصائيات اليومية لليوم الحالي
                var today = DateTime.Today;
                var statsSpec = new UserDailyStatsSpecification(userId, today);
                var todayStats = await _unitOfWork.Repository<UserDailyStats>().GetEntityWithSpecAsync(statsSpec);
                var statsDto = new DashboardStatsDto
                {
                    AttendanceDates = attendanceDates,
                    CaloriesBurned = todayStats?.CaloriesBurned ?? 0,
                    TotalCalories = todayStats?.TotalCalories ?? 500, // قيمة افتراضية
                    Steps = todayStats?.Steps ?? 0,
                    TotalSteps = todayStats?.TotalSteps ?? 10000, // قيمة افتراضية
                    WaterIntake = todayStats?.WaterIntake ?? 0,
                    TotalWater = todayStats?.TotalWater ?? 3000 // قيمة افتراضية
                };

                // جلب تاريخ الوزن (آخر 30 يوم مثلاً)
                var startDate = today.AddDays(-30);
                var weightSpec = new UserWeightHistorySpecification(userId, startDate, today);
                var weightHistory = await _unitOfWork.Repository<UserDailyStats>().GetAllWithSpecAsync(weightSpec);
                statsDto.WeightHistory = weightHistory.Select(w => new WeightEntryDto
                {
                    Date = w.Date,
                    Weight = w.Weight
                }).ToList();

                return new ApiResponse(200, "Dashboard stats retrieved successfully", statsDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to retrieve dashboard stats: {ex.Message}");
            }
        }

        public async Task<ApiResponse> AddDailyStats(string userId, AddDailyStatsDto statsDto)
        {
            try
            {
                if (statsDto.Date.Date != DateTime.Today)
                    return new ApiResponse(400, "You can only add stats for today.");

                if (statsDto.CaloriesBurned < 0 || statsDto.Steps < 0 || statsDto.WaterIntake < 0 || statsDto.Weight <= 0)
                    return new ApiResponse(400, "Invalid stats values: Values cannot be negative, and weight must be greater than 0.");

                var statsSpec = new UserDailyStatsSpecification(userId, statsDto.Date);
                var existingStats = await _unitOfWork.Repository<UserDailyStats>().GetEntityWithSpecAsync(statsSpec);

                if (existingStats != null)
                {
                    // تحديث الإحصائيات الموجودة
                    existingStats.CaloriesBurned = statsDto.CaloriesBurned;
                    existingStats.Steps = statsDto.Steps;
                    existingStats.WaterIntake = statsDto.WaterIntake;
                    existingStats.Weight = statsDto.Weight;

                    _unitOfWork.Repository<UserDailyStats>().Update(existingStats);
                }
                else
                {
                    // إضافة إحصائيات جديدة
                    var newStats = new UserDailyStats
                    {
                        UserId = userId,
                        Date = statsDto.Date.Date, // التأكد إن التاريخ بدون وقت
                        CaloriesBurned = statsDto.CaloriesBurned,
                        TotalCalories = 500, // قيمة افتراضية (ممكن تجيبيها من إعدادات المستخدم)
                        Steps = statsDto.Steps,
                        TotalSteps = 10000, // قيمة افتراضية
                        WaterIntake = statsDto.WaterIntake,
                        TotalWater = 3000, // قيمة افتراضية
                        Weight = statsDto.Weight
                    };

                    await _unitOfWork.Repository<UserDailyStats>().Add(newStats);
                }

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                    return new ApiExceptionResponse(500, "Failed to save daily stats due to database error.");

                var updatedStats = await _unitOfWork.Repository<UserDailyStats>().GetEntityWithSpecAsync(statsSpec);
                var statsDtoResponse = _mapper.Map<DashboardStatsDto>(updatedStats);
                return new ApiResponse(200, "Daily stats saved successfully", statsDtoResponse);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Failed to save daily stats: {ex.Message}");
            }
        }
    }
}