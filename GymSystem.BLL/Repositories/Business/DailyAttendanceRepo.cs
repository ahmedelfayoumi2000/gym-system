using AutoMapper;
using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using GymSystem.BLL.Specifications.AttendanceSpec;
using GymSystem.BLL.Specifications.MembershipSpec;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace GymSystem.BLL.Repositories.Business
{
    public class DailyAttendanceRepo : IDailyAttendanceRepo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyAttendanceRepo> _logger;
        private readonly IMembershipRepo _membershipRepo;
        private readonly UserManager<AppUser> _userManager;

        public DailyAttendanceRepo(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<DailyAttendanceRepo> logger,
            IMembershipRepo membershipRepo,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _membershipRepo = membershipRepo ?? throw new ArgumentNullException(nameof(membershipRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<ApiResponse> AddAttendanceAsync(AttendanceDto attendanceDto)
        {
            if (attendanceDto == null || string.IsNullOrWhiteSpace(attendanceDto.UserCode))
            {
                return new ApiResponse(400, "Attendance data or UserCode cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Adding attendance for UserCode: {UserCode}", attendanceDto.UserCode);

                var membership = await ValidateMembership(attendanceDto.UserCode);
                if (membership == null)
                {
                    return new ApiResponse(404, "User not found");
                }

                var validationResult = await ValidateAttendance(attendanceDto.UserCode, membership);
                if (validationResult != null)
                {
                    return validationResult;
                }

                await UpdateMembershipDays(membership);

                var attendance = CreateAttendance(attendanceDto, membership);
                await _unitOfWork.Repository<Attendance>().Add(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to save attendance for UserCode {UserCode}", attendanceDto.UserCode);
                    return new ApiResponse(500, "Failed to save attendance");
                }

                _logger.LogInformation("Attendance added successfully for UserCode: {UserCode}", attendanceDto.UserCode);
                var createdDto = _mapper.Map<AttendanceDto>(attendance);
                return new ApiResponse(201, "Attendance added successfully", createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attendance for UserCode: {UserCode}", attendanceDto.UserCode);
                return new ApiExceptionResponse(500, $"Error adding attendance: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<Attendance>> GetAllWithSpecAsync(ISpecification<Attendance> spec)
        {
            try
            {
                _logger.LogInformation("Retrieving attendances with specification");
                return await _unitOfWork.Repository<Attendance>().GetAllWithSpecAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendances with specification");
                throw;
            }
        }

        public async Task<int> GetCountAsync(ISpecification<Attendance> spec)
        {
            try
            {
                _logger.LogInformation("Counting attendances with specification");
                return await _unitOfWork.Repository<Attendance>().GetCountAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting attendances with specification");
                throw;
            }
        }

        public async Task<IReadOnlyList<AttendanceDto>> GetAttendancesForUserAsync(string userCode)
        {
            try
            {
                _logger.LogInformation("Retrieving attendances for userCode: {userCode}", userCode);

                var membership = await ValidateMembership(userCode);
                if (membership == null)
                {
                    _logger.LogWarning("Membership for UserCode {userCode} not found", userCode);
                    return new List<AttendanceDto>().AsReadOnly();
                }

                var spec = new AttendanceByUserCodeSpec(userCode);
                var attendances = await _unitOfWork.Repository<Attendance>().GetAllWithSpecAsync(spec);

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
                _logger.LogInformation("Deleting attendance with ID: {AttendanceId}", id);

                var attendance = await _unitOfWork.Repository<Attendance>().GetByIdAsync(id);
                if (attendance == null)
                {
                    _logger.LogWarning("Attendance with ID {AttendanceId} not found.", id);
                    return new ApiResponse(404, "Attendance not found");
                }

                _unitOfWork.Repository<Attendance>().Delete(attendance);

                var result = await _unitOfWork.Complete();
                if (result <= 0)
                {
                    _logger.LogError("Failed to delete attendance with ID {AttendanceId}", id);
                    return new ApiResponse(500, "Failed to delete attendance");
                }

                _logger.LogInformation("Attendance deleted successfully with ID: {AttendanceId}", id);
                return new ApiResponse(200, "Attendance deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance with ID: {AttendanceId}", id);
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

                if (user.IsProfileConfirmed == false)
                {
                    throw new SecurityException($"User with ID {userId} profile is not confirmed.");
                }

                return await GenerateQRCodeForUser(userId);
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
            if (checkInDto == null || string.IsNullOrWhiteSpace(checkInDto.UserCode))
            {
                return new ApiResponse(400, "Check-in data or UserCode cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new ApiResponse(401, "Authenticated UserCode is required.");
            }

            try
            {
                var membership = await ValidateMembership(checkInDto.UserCode);
                if (membership == null)
                {
                    return new ApiResponse(404, "User not found");
                }

                var user = await ValidateUser(checkInDto.UserCode);
                if (user == null)
                {
                    return new ApiResponse(404, $"User with code {checkInDto.UserCode} not found.");
                }

                var currentUser = await ValidateCurrentUser(currentUserId);
                if (currentUser == null)
                {
                    return new ApiResponse(401, $"Current user with ID {currentUserId} not found.");
                }

                var validationResult = await ValidateCheckIn(checkInDto.UserCode, membership);
                if (validationResult != null)
                {
                    return validationResult;
                }

                await UpdateMembershipDays(membership);

                var attendance = CreateCheckInAttendance(checkInDto, currentUserId, membership);
                await _unitOfWork.Repository<Attendance>().Add(attendance);

                var saveResult = await _unitOfWork.Complete();
                if (saveResult <= 0)
                {
                    return new ApiResponse(500, "Failed to persist the check-in record.");
                }

                var attendanceDto = _mapper.Map<AttendanceViewDto>(attendance);
                attendanceDto.CreatedByUserName = currentUser.DisplayName;

                return new ApiResponse(201, "Check-in recorded successfully", attendanceDto);
            }
            catch (Exception ex) when (ex is ArgumentException or SecurityException)
            {
                _logger.LogError(ex, "Validation or security error during check-in for UserCode: {UserCode}", checkInDto?.UserCode ?? "Unknown");
                throw;
            }
            catch (Exception ex)
            {
                return new ApiExceptionResponse(500, "An unexpected error occurred during check-in.", ex.Message);
            }
        }

        #region Private Helper Methods

        private async Task<Membership> ValidateMembership(string userCode)
        {
            var spec = new MembershipByUserCodeSpecification(userCode);
            var membership = await _unitOfWork.Repository<Membership>().GetEntityWithSpecAsync(spec);
            if (membership == null)
            {
                _logger.LogWarning("Membership for UserCode {UserCode} not found", userCode);
                return null;
            }

            if (membership.StopDate.HasValue && membership.StopDate.Value < DateTime.UtcNow)
            {
                membership.StopDate = null;
                _unitOfWork.Repository<Membership>().Update(membership);
                await _unitOfWork.Complete();
            }

            return membership;
        }

        private async Task<ApiResponse> ValidateAttendance(string userCode, Membership membership)
        {
            var today = DateTime.UtcNow.Date;
            var attendanceSpec = new AttendanceByUserCodeAndDateSpecification(userCode, today);
            var existingAttendance = await _unitOfWork.Repository<Attendance>().GetEntityWithSpecAsync(attendanceSpec);
            if (existingAttendance != null)
            {
                _logger.LogWarning("UserCode {UserCode} has already checked in today.", userCode);
                return new ApiResponse(409, "You have already checked in today. You can only check in once per day.");
            }

            if (!membership.IsActive)
            {
                _logger.LogWarning("Membership for UserCode {UserCode} not Active", userCode);
                return new ApiResponse(403, "User is not Active. Please subscribe to a new plan to continue.");
            }

            if (membership.StopDate.HasValue && DateTime.UtcNow < membership.StopDate.Value)
            {
                return new ApiResponse(403, $"You are still suspended until {membership.StopDate.Value:yyyy-MM-dd}. Please wait until your suspension period ends.");
            }

            if (membership.EndDate < DateTime.UtcNow)
            {
                membership.IsActive = false;
                _unitOfWork.Repository<Membership>().Update(membership);
                _logger.LogWarning("Membership for UserCode {UserCode} has expired on {EndDate}.", userCode, membership.EndDate);
                return new ApiResponse(403, $"Your membership expired on {membership.EndDate:yyyy-MM-dd}. Please renew your plan.");
            }

            return null;
        }

        private async Task<ApiResponse> ValidateCheckIn(string userCode, Membership membership)
        {
            return await ValidateAttendance(userCode, membership);
        }

        private async Task<AppUser> ValidateUser(string userCode)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserCode == userCode);
            if (user == null)
            {
                return null;
            }

            if (user.IsProfileConfirmed == false)
            {
                _logger.LogWarning("User with Code {UserCode} profile is not confirmed.", userCode);
                throw new SecurityException($"User with Code {userCode} profile is not confirmed.");
            }

            return user;
        }

        private async Task<AppUser> ValidateCurrentUser(string currentUserId)
        {
            return await _userManager.FindByIdAsync(currentUserId);
        }

        private async Task UpdateMembershipDays(Membership membership)
        {
            if (membership.HaveDays > 0)
            {
                membership.HaveDays -= 1;
            }
            if (membership.HaveDays == 0)
            {
                membership.IsActive = false;
            }
            _unitOfWork.Repository<Membership>().Update(membership);
            await _unitOfWork.Complete();
        }

        private Attendance CreateAttendance(AttendanceDto attendanceDto, Membership membership)
        {
            var attendance = _mapper.Map<Attendance>(attendanceDto);
            attendance.Membership = membership;
            attendance.MembershipId = membership.Id;
            return attendance;
        }

        private Attendance CreateCheckInAttendance(AttendanceCheckInDto checkInDto, string currentUserId, Membership membership)
        {
            return new Attendance
            {
                UserCode = checkInDto.UserCode,
                AttendanceDate = DateTime.UtcNow,
                CreatedByUserId = currentUserId,
                Membership = membership,
                MembershipId = membership.Id
            };
        }

        private async Task<QRCodeDto> GenerateQRCodeForUser(string userId)
        {
            await Task.Yield();

            var qrCodeWriter = new BarcodeWriterPixelData
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

            var pixelData = qrCodeWriter.Write(userId);

            using var bitmap = new SkiaSharp.SKBitmap(new SkiaSharp.SKImageInfo(pixelData.Width, pixelData.Height));
            IntPtr ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(pixelData.Pixels, 0);
            bitmap.InstallPixels(bitmap.Info, ptr);

            using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using var memoryStream = new MemoryStream();

            encoded.SaveTo(memoryStream);
            string qrCodeBase64 = Convert.ToBase64String(memoryStream.ToArray());

            return new QRCodeDto { QRCodeBase64 = qrCodeBase64 };
        }

        #endregion
    }
}