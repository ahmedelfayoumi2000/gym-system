using AutoMapper;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.AttendanceByUserCode;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;
using System.Drawing;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using System.Security;

namespace GymSystem.BLL.Repositories.Business
{
    public class DailyAttendanceRepo : IDailyAttendanceRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyAttendanceRepo> _logger;

        public DailyAttendanceRepo(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<DailyAttendanceRepo> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userManager = userManager;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        public async Task<ApiResponse> AddAttendanceAsync(AttendanceDto attendanceDto)
        {
            try
            {

                var membershipSpec = new BaseSpecification<MonthlyMembership>(m => m.UserCode == attendanceDto.UserCode && !m.IsDeleted);
                var membershipRepo = _unitOfWork.Repository<MonthlyMembership>();
                var existingMembership = await membershipRepo.GetEntityWithSpecAsync(membershipSpec);

                if (existingMembership == null)
                {
                    _logger.LogWarning("Add attendance aborted: Membership for UserCode {UserCode} not found.", attendanceDto.UserCode);
                    return new ApiResponse(404, $"Membership for UserCode {attendanceDto.UserCode} not found.");
                }

                if (!existingMembership.IsActive)
                {
                    return new ApiResponse(403, "User is not active. Please subscribe to a new plan to continue.");
                }

                if (existingMembership.StopDate.HasValue && DateTime.UtcNow < existingMembership.StopDate.Value)
                {
                    return new ApiResponse(403, $"You are still suspended until {existingMembership.StopDate.Value:yyyy-MM-dd}. Please wait until your suspension period ends.");
                }

                if (existingMembership.EndDate < DateTime.UtcNow)
                {
                    existingMembership.IsActive = false;
                    membershipRepo.Update(existingMembership);
                    await _unitOfWork.Complete();
                    return new ApiResponse(403, $"Your membership expired on {existingMembership.EndDate:yyyy-MM-dd}. Please renew your plan.");
                }

                existingMembership.HaveDays -= 1;
                if (existingMembership.HaveDays <= 0)
                {
                    existingMembership.IsActive = false;
                    existingMembership.HaveDays = 0; 
                }

                membershipRepo.Update(existingMembership);

                var attendance = _mapper.Map<Attendance>(attendanceDto);
                attendance.Membership = existingMembership;
                attendance.MembershipId = existingMembership.Id;

                var attendanceRepo = _unitOfWork.Repository<Attendance>();
                await attendanceRepo.Add(attendance);

                var saveResult = await _unitOfWork.Complete();
                if (saveResult <= 0)
                {
                    return new ApiResponse(500, "Failed to save attendance to the database.");
                }

                var createdDto = _mapper.Map<AttendanceDto>(attendance);
                return new ApiResponse(201, "Attendance added successfully", createdDto);
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An unexpected error occurred while adding attendance.", ex.Message);
            }
        }

       
        public async Task<IReadOnlyList<AttendanceDto>> GetAttendancesForUserAsync(string userCode)
        {
            try
            {
                _logger.LogInformation("Retrieving attendances for userCode: {userCode}", userCode);


                var spec = new BaseSpecification<MonthlyMembership>(m => m.UserCode == userCode);
                var existingMembership = await _unitOfWork.Repository<MonthlyMembership>().GetEntityWithSpecAsync(spec);
                if (existingMembership == null)
                {
                    _logger.LogWarning("Membership for UserCode {userCode} not found ", userCode);
                    return new List<AttendanceDto>().AsReadOnly();
                }

                var attendanceRepo = _unitOfWork.Repository<Attendance>();
                var spec2 = new AttendanceByUserCodeSpec(userCode);
                var attendances = await attendanceRepo.GetAllWithSpecAsync(spec2);

                var attendanceDtos = _mapper.Map<IReadOnlyList<AttendanceDto>>(attendances);
                _logger.LogInformation("Retrieved {Count} attendances for userCode: {userCode}", attendanceDtos.Count, userCode);

                return attendanceDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendances for userCode: {userCode}", userCode);
                return new List<AttendanceDto>().AsReadOnly();
            }
        }

        public async Task<ApiResponse> DeleteAttendanceAsync(int id)
        {
            try
            {
                var attendanceRepo = _unitOfWork.Repository<Attendance>();
                var attendance = await attendanceRepo.GetByIdAsync(id);
                if (attendance == null)
                {
                    return new ApiResponse(404, "Attendance not found");
                }

                attendanceRepo.Delete(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    return new ApiResponse(500, "Failed to delete attendance");
                }

                return new ApiResponse(200, "Attendance deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, $"Error deleting attendance: {ex.Message}");
            }
        }

        public async Task<QRCodeDto> GenerateQRCodeAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID must be a non-empty string.", nameof(userId));
            }

            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new SecurityException($"User with ID {userId} does not exist.");
                }

                if (!user.IsProfileConfirmed)
                {
                    throw new SecurityException($"User with ID {userId} profile is not confirmed.");
                }

                await Task.Yield();

                // Configure QR Code generation 
                var qrCodeWriter = new BarcodeWriter<SkiaSharp.SKBitmap>
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = 250,
                        Width = 250,
                        Margin = 1,
                        ErrorCorrection = ErrorCorrectionLevel.Q
                    }
                };

                using var qrCodeImage = qrCodeWriter.Write(userId);
                using var memoryStream = new MemoryStream();

                // Convert SKBitmap to PNG and encode to Base64
                using (var skImage = SkiaSharp.SKImage.FromBitmap(qrCodeImage))
                using (var skData = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    skData.SaveTo(memoryStream);
                }

                string qrCodeBase64 = Convert.ToBase64String(memoryStream.ToArray());

                return new QRCodeDto { QRCodeBase64 = qrCodeBase64 };
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException or InvalidOperationException)
            {
                _logger.LogError(ex, "QR Code generation failed for user ID: {UserId} due to a handled exception.", userId);
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occurred while generating the QR Code.", ex);
            }
        }

      
        public async Task<ApiResponse> CheckInAsync(AttendanceCheckInDto checkInDto, string currentUserId)
        {
            if (checkInDto == null)
            {
                return new ApiResponse(400, "Check-in data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(checkInDto.UserId))
            {
                return new ApiResponse(400, "User ID cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new ApiResponse(401, "Authenticated user ID is required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(checkInDto.UserId);
                if (user == null)
                {
                    return new ApiResponse(404, $"User with ID {checkInDto.UserId} not found.");
                }

                if (!user.IsProfileConfirmed)
                {
                    return new ApiResponse(403, $"User with ID {checkInDto.UserId} profile is not confirmed.");
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                var today = DateTime.UtcNow.Date;
                var attendanceSpec = new BaseSpecification<Attendance>(a =>
                    a.UserId == checkInDto.UserId &&
                    !a.IsDeleted &&
                    a.CheckInTime.Date == today);

                var attendanceRepo = _unitOfWork.Repository<Attendance>();
                var existingCheckIn = await attendanceRepo.GetEntityWithSpecAsync(attendanceSpec);
                if (existingCheckIn != null)
                {
                    return new ApiResponse(409, $"User with ID {checkInDto.UserId} has already checked in today.");
                }

                // Record the check-in
                var attendance = new Attendance
                {
                    UserId = checkInDto.UserId,
                    CheckInTime = DateTime.UtcNow,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await attendanceRepo.Add(attendance);
                var saveResult = await _unitOfWork.Complete();

                if (saveResult <= 0)
                {
                    return new ApiResponse(500, "Failed to persist the check-in record.");
                }

                var attendanceDto = _mapper.Map<AttendanceViewDto>(attendance);
                attendanceDto.UserName = user.DisplayName;
                attendanceDto.CreatedByUserName = currentUser.DisplayName;

                return new ApiResponse(201, "Check-in recorded successfully", attendanceDto);
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                _logger.LogError(ex, "Validation or security error during check-in for user ID: {UserId}", checkInDto?.UserId ?? "Unknown");
                throw;
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An unexpected error occurred during check-in.", ex.Message);
            }
        }


    }
}