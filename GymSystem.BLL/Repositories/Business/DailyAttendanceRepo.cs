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

        //public async Task<QRCodeDto> GenerateQRCodeAsync(string userId)
        //{
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        _logger.LogWarning("Invalid user ID for generating QR Code.");
        //        throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        //    }

        //    try
        //    {
        //        _logger.LogInformation("Generating QR Code for user ID: {UserId}", userId);

        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null || !user.IsProfileConfirmed)
        //        {
        //            _logger.LogWarning("User with ID {UserId} not found or profile not confirmed.", userId);
        //            throw new ApplicationException($"User with ID {userId} not found or profile not confirmed.");
        //        }

        //        // Generate QR Code using QRCoder
        //        using var qrGenerator = new QRCodeGenerator();
        //        var qrCodeData = qrGenerator.CreateQrCode(userId, QRCodeGenerator.ECCLevel.Q);
        //        using var qrCode = new QRCode(qrCodeData);
        //        using var qrCodeImage = qrCode.GetGraphic(20); // 20 pixels per module

        //        // Convert Bitmap to Base64
        //        using var ms = new MemoryStream();
        //        qrCodeImage.Save(ms, ImageFormat.Png); // استخدام ImageFormat.Png
        //        var qrCodeBase64 = Convert.ToBase64String(ms.ToArray());

        //        _logger.LogInformation("QR Code generated successfully for user ID: {UserId}", userId);
        //        return new QRCodeDto { QRCodeBase64 = qrCodeBase64 };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating QR Code for user ID: {UserId}", userId);
        //        throw new ApplicationException($"Failed to generate QR Code: {ex.Message}", ex);
        //    }
        //}

        public async Task<QRCodeDto> GenerateQRCodeAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Attempted to generate QR Code with invalid user ID.");
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsProfileConfirmed)
                {
                    _logger.LogWarning("User with ID {UserId} not found or profile not confirmed.", userId);
                    throw new ApplicationException($"User with ID {userId} not found or profile not confirmed.");
                }

                await Task.Yield();

                // Configure QR Code generation using ZXing.Net
                var qrCodeWriter = new BarcodeWriter<Bitmap>
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

                // Generate QR Code and convert to Base64
                using var qrCodeImage = qrCodeWriter.Write(userId);
                using var memoryStream = new MemoryStream();
                qrCodeImage.Save(memoryStream, ImageFormat.Png);
                string qrCodeBase64 = Convert.ToBase64String(memoryStream.ToArray());

                _logger.LogInformation("QR Code generated successfully for user ID: {UserId}", userId);
                return new QRCodeDto { QRCodeBase64 = qrCodeBase64 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR Code for user ID: {UserId}", userId);
                throw new ApplicationException("An error occurred while generating the QR Code.", ex);
            }
        }


        public async Task<ApiResponse> CheckInAsync(AttendanceCheckInDto checkInDto, string currentUserId)
        {
            if (checkInDto == null || string.IsNullOrEmpty(checkInDto.UserId))
            {
                _logger.LogWarning("Invalid check-in data or user ID.");
                return new ApiResponse(400, "Check-in data or user ID cannot be null.");
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Current user ID is missing for check-in.");
                return new ApiResponse(401, "User authentication required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(checkInDto.UserId);
                if (user == null || !user.IsProfileConfirmed)
                {
                    _logger.LogWarning("User with ID {UserId} not found or profile not confirmed.", checkInDto.UserId);
                    return new ApiResponse(404, $"User with ID {checkInDto.UserId} not found or profile not confirmed.");
                }

                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    _logger.LogWarning("Current user with ID {CurrentUserId} not found.", currentUserId);
                    return new ApiResponse(404, $"Current user with ID {currentUserId} not found.");
                }

                // Check if the user already checked in today
                var today = DateTime.UtcNow.Date;
                var spec = new BaseSpecification<Attendance>(a =>
                    a.UserId == checkInDto.UserId &&
                    !a.IsDeleted &&
                    a.CheckInTime.Date == today);
                var existingCheckIn = await _unitOfWork.Repository<Attendance>().GetEntityWithSpecAsync(spec);
                if (existingCheckIn != null)
                {
                    _logger.LogWarning("User with ID {UserId} already checked in today.", checkInDto.UserId);
                    return new ApiResponse(409, $"User with ID {checkInDto.UserId} already checked in today.");
                }

                var attendance = new Attendance
                {
                    UserId = checkInDto.UserId,
                    CheckInTime = DateTime.UtcNow,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Repository<Attendance>().Add(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save check-in for user ID: {UserId}", checkInDto.UserId);
                    return new ApiResponse(500, "Failed to save the check-in to the database.");
                }

                var attendanceDto = _mapper.Map<AttendanceViewDto>(attendance);
                attendanceDto.UserName = user.DisplayName;
                attendanceDto.CreatedByUserName = currentUser.DisplayName;

                _logger.LogInformation("User with ID {UserId} checked in successfully.", checkInDto.UserId);
                return new ApiResponse(201, "Check-in recorded successfully", attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in user with ID: {UserId}", checkInDto.UserId);
                return new ApiExceptionResponse(500, "An error occurred while recording the check-in", ex.Message);
            }
        }

       
    }
}