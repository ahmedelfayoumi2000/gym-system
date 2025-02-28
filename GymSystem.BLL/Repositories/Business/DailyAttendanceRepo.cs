using AutoMapper;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications.AttendanceByUserCode;
using GymSystem.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.BLL.Repositories.Business
{
    public class DailyAttendanceRepo : IDailyAttendanceRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyAttendanceRepo> _logger;

        public DailyAttendanceRepo(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<DailyAttendanceRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a new daily attendance record for a user identified by UserCode.
        /// </summary>
        public async Task<ApiResponse> AddAttendanceAsync(DailyAttendanceDto attendanceDto)
        {
            try
            {
                _logger.LogInformation("Adding daily attendance for UserCode: {UserCode}", attendanceDto.UserCode);

                var user = await _userRepository.GetUserByCodeAsync(attendanceDto.UserCode);
                if (user == null)
                {
                    _logger.LogWarning("User with UserCode {UserCode} not found.", attendanceDto.UserCode);
                    return new ApiResponse(404, "User not found");
                }

                attendanceDto.UserId = user.Id;
                attendanceDto.IsPresent = true; // تعيين الحضور كـ "حاضر" افتراضيًا
                var attendance = _mapper.Map<DailyAttendance>(attendanceDto);

                var attendanceRepo = _unitOfWork.Repository<DailyAttendance>();
                await attendanceRepo.Add(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save daily attendance for UserCode {UserCode}", attendanceDto.UserCode);
                    return new ApiResponse(500, "Failed to save attendance");
                }

                _logger.LogInformation("Daily attendance added successfully for UserCode: {UserCode}", attendanceDto.UserCode);
                var createdDto = _mapper.Map<DailyAttendanceDto>(attendance);
                return new ApiResponse(201, "Daily attendance added successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding daily attendance for UserCode: {UserCode}", attendanceDto.UserCode);
                return new ApiExceptionResponse(500, $"Error adding daily attendance: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all daily attendance records for a user identified by UserCode.
        /// </summary>
        public async Task<IReadOnlyList<DailyAttendanceDto>> GetAttendancesForUserAsync(string userCode)
        {
            try
            {
                _logger.LogInformation("Retrieving daily attendances for UserCode: {UserCode}", userCode);

                var user = await _userRepository.GetUserByCodeAsync(userCode);
                if (user == null)
                {
                    _logger.LogWarning("User with UserCode {UserCode} not found.", userCode);
                    return new List<DailyAttendanceDto>().AsReadOnly();
                }

                var attendanceRepo = _unitOfWork.Repository<DailyAttendance>();
                var spec = new AttendanceByUserCodeSpec(userCode);
                var attendances = await attendanceRepo.GetAllWithSpecAsync(spec);

                var attendanceDtos = _mapper.Map<IReadOnlyList<DailyAttendanceDto>>(attendances);
                _logger.LogInformation("Retrieved {Count} daily attendances for UserCode: {UserCode}", attendanceDtos.Count, userCode);

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily attendances for UserCode: {UserCode}", userCode);
                return new List<DailyAttendanceDto>().AsReadOnly();
            }
        }

        /// <summary>
        /// Deletes a daily attendance record by its ID.
        /// </summary>
        public async Task<ApiResponse> DeleteAttendanceAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting daily attendance with ID: {AttendanceId}", id);

                var attendanceRepo = _unitOfWork.Repository<DailyAttendance>();
                var attendance = await attendanceRepo.GetByIdAsync(id);
                if (attendance == null)
                {
                    _logger.LogWarning("Daily attendance with ID {AttendanceId} not found.", id);
                    return new ApiResponse(404, "Attendance not found");
                }

                attendanceRepo.Delete(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to delete daily attendance with ID {AttendanceId}", id);
                    return new ApiResponse(500, "Failed to delete attendance");
                }

                _logger.LogInformation("Daily attendance deleted successfully with ID: {AttendanceId}", id);
                return new ApiResponse(200, "Daily attendance deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting daily attendance with ID: {AttendanceId}", id);
                return new ApiExceptionResponse(500, $"Error deleting daily attendance: {ex.Message}");
            }
        }
    }
}