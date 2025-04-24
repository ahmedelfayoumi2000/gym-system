using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.User;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Auth;
using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities.Identity;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GymSystem.BLL.Services.Auth
{
    public class AccountService : IAccountService
    {
        #region Fields
        private readonly UserManager<AppUser> _userManager;
        private readonly MailSettings _mailSettings;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;
        private readonly IMemoryCache _memoryCache;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountService> _logger;
        private readonly ActiveUserManager _activeUserManager;
        #endregion

        #region Constructor
        public AccountService(
            UserManager<AppUser> userManager,
            IOptionsMonitor<MailSettings> options,
            ITokenService tokenService,
            IOtpService otpService,
            IMemoryCache memoryCache,
            SignInManager<AppUser> signInManager,
            ILogger<AccountService> logger,
            ActiveUserManager activeUserManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mailSettings = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activeUserManager = activeUserManager;
        }
        #endregion

        #region Implementation of IAccountService
        public async Task<ApiResponse> RegisterAsync(Register dto, Func<string, string, string> generateCallBackUrl)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return new ApiResponse(400, "User with this email already exists.");
                }

                var userCode = await GenerateUserCodeAsync(dto.MembershipType);
                var user = CreateUserFromDto(dto, userCode);

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return new ApiResponse(400, $"Failed to create user. Errors: {errors}");
                }

                var roleResult = await AssignRoleToUserAsync(user, dto.UserRole);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return new ApiResponse(400, $"Failed to assign role. Errors: {roleErrors}");
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = generateCallBackUrl(emailConfirmationToken, user.Id);
                var emailBody = BuildEmailBody(user.UserName, callbackUrl);

                await SendEmailAsync(user.Email, "Email Confirmation", emailBody);

                var (jwtToken, refreshToken) = await _tokenService.CreateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                return new ApiResponse(200, "Email verification has been sent successfully. Please verify it!",
                    new UserDto
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender,
                        Age = user.Age,
                        Roles = roles.ToList(),
                        Token = jwtToken,
                        RefreshToken = refreshToken.Token,
                        UserCode = user.UserCode,
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", dto.Email);
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return new ApiResponse(500, $"An error occurred: {errorMessage}");
            }
        }

        public async Task<ApiResponse> LoginAsync(Login dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new ApiResponse(400, "Email and password are required.");
            }

            if (dto.Type == null)
            { dto.Type = Types.Windows; }

            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return new ApiResponse(400, "User not found.");
                }


                if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    return new ApiResponse(400, "Incorrect email or password.");
                }

                if (!user.EmailConfirmed)
                {
                    return new ApiResponse(400, "Email not confirmed. Please verify your email address.");
                }

                //if (_activeUserManager.IsUserLoggedIn(user.Id))
                //{
                //	return new ApiResponse(403, "User is already logged in. Please log out first.");
                //}

                var (jwtToken, refreshToken) = await _tokenService.CreateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    return new ApiResponse(403, "User has no roles assigned.");
                }

                if (dto.Type == Types.Windows)
                {
                    // Windows: مسموح لـ Admin و Receptionist فقط
                    if (!roles.Contains(UserRoleEnum.Admin.ToString()) && !roles.Contains(UserRoleEnum.Receptionist.ToString()))
                    {
                        return new ApiResponse(403, "Access denied: Only Admin and Receptionist can log in on Windows.");
                    }
                }
                else if (dto.Type == Types.Mobile)
                {
                    if (!roles.Contains(UserRoleEnum.Admin.ToString()) && !roles.Contains(UserRoleEnum.Trainer.ToString()) && !roles.Contains(UserRoleEnum.Member.ToString()))
                    {
                        return new ApiResponse(403, "Access denied: Only Admin, Trainer, and Member can log in on Mobile.");
                    }
                }

                //_activeUserManager.AddUser(user.Id);

                return new ApiResponse(200, "Login successful",
                    new UserDto
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender,
                        Height = user.Height,
                        Weight = user.Weight,
                        Age = user.Age,
                        FitnessLevel = user.FitnessLevel.ToString(),
                        Goal = user.Goal.ToString(),
                        IsProfileConfirmed = user.IsProfileConfirmed,
                        Roles = roles.ToList(),
                        Token = jwtToken,
                        RefreshToken = refreshToken.Token,
                        UserCode = user.UserCode
                    });
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse> LogoutAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse(400, "User ID is required.");
            }

            try
            {
                if (!_activeUserManager.IsUserLoggedIn(userId))
                {
                    return new ApiResponse(400, "User is not currently logged in.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse(404, "User not found.");
                }

                _activeUserManager.RemoveUser(userId);

                return new ApiResponse(200, "Logout successful");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"An error occurred while logging out: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ForgetPassword(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ApiResponse(400, "User not found.");
                }

                var otp = _otpService.GenerateOtp(email);
                var emailBody = $"<h1>Your Verification Code is: {otp}</h1><p>Use this code to reset your password. It expires in 5 minutes.</p>";

                await SendEmailAsync(email, "Verification Code", emailBody);

                return new ApiResponse(200, "Password reset email sent successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Failed to send reset email: {ex.Message}");
            }
        }

        public ApiResponse VerfiyOtp(VerifyOtp dto)
        {
            try
            {
                var isValidOtp = _otpService.IsValidOtp(dto.Email, dto.Otp);
                if (!isValidOtp)
                {
                    return new ApiResponse(400, "Invalid OTP.");
                }

                _memoryCache.Set(dto.Email, true, TimeSpan.FromMinutes(5));
                return new ApiResponse(200, "OTP verified successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPassword dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return new ApiResponse(400, "User not found.");
                }

                if (!_memoryCache.TryGetValue(dto.Email, out bool isOtpValid) || !isOtpValid)
                {
                    return new ApiResponse(400, "Please verify your email with OTP first.");
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.Password);

                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    return new ApiResponse(400, $"Failed to reset password: {errors}");
                }

                _memoryCache.Remove(dto.Email);
                return new ApiResponse(200, "Password reset successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new ApiResponse(404, "User not found.");
                }

                var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, oldPassword);
                if (!isOldPasswordValid)
                {
                    return new ApiResponse(400, "The old password is incorrect.");
                }

                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new ApiResponse(400, $"Failed to change password: {errors}");
                }

                await _signInManager.SignOutAsync();
                return new ApiResponse(200, "Password changed successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ResendConfirmationEmailAsync(string email, Func<string, string, string> generateCallBackUrl)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new ApiResponse(400, "User with this email does not exist.");
                }

                if (user.EmailConfirmed)
                {
                    return new ApiResponse(400, "Email is already confirmed.");
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = generateCallBackUrl(emailConfirmationToken, user.Id);
                var emailBody = BuildEmailBody(user.UserName, callbackUrl);

                await SendEmailAsync(user.Email, "Email Confirmation", emailBody);

                return new ApiResponse(200, "Email verification has been resent successfully. Please verify it!");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, $"Failed to resend confirmation email: {ex.Message}");
            }
        }

        public async Task<(bool Succeeded, string ErrorMessage)> ConfirmUserEmailAsync(string userId, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return (false, "User ID and confirmation token are required.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                if (user.EmailConfirmed)
                {
                    return (false, "Email is already confirmed.");
                }

                var confirmed = await _userManager.ConfirmEmailAsync(user, token);
                if (!confirmed.Succeeded)
                {
                    var errors = string.Join(", ", confirmed.Errors.Select(e => e.Description));
                    return (false, $"Failed to confirm email: {errors}");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"An unexpected error occurred: {ex.Message}");
            }
        }
        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellation = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.DisplayedName, _mailSettings.Email));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port, SecureSocketOptions.StartTls, cancellation);
                await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password, cancellation);
                await client.SendAsync(message, cancellation);
                await client.DisconnectAsync(true, cancellation);

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
        #endregion

        #region Private Helper Methods
        private async Task<string> GenerateUserCodeAsync(MembershipType membershipType)
        {
            var currentUserCount = await _userManager.Users.CountAsync();
            return GenerateUserCode(membershipType, currentUserCount);
        }

        private string GenerateUserCode(MembershipType membershipType, int currentUserCount)
        {
            char prefix = membershipType switch
            {
                MembershipType.Gold_1Month or MembershipType.Gold_3Months or MembershipType.Gold_6Months => 'G',
                MembershipType.Silver_1Month or MembershipType.Silver_3Months or MembershipType.Silver_6Months => 'S',
                MembershipType.Platinum_1Month or MembershipType.Platinum_3Months or MembershipType.Platinum_6Months => 'P',
                MembershipType.Bronze_1Month or MembershipType.Bronze_3Months or MembershipType.Bronze_6Months => 'B',
                MembershipType.Diamond_1Month or MembershipType.Diamond_3Months or MembershipType.Diamond_6Months => 'D',
                _ => 'U'
            };
            return $"{prefix}{currentUserCount + 1}";
        }

        private AppUser CreateUserFromDto(Register dto, string userCode)
        {
            return new AppUser
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                UserName = dto.Email.Split('@')[0],
                UserRole = (int)dto.UserRole,
                EmailConfirmed = false,
                Gender = dto.Gender,
                UserCode = userCode
            };
        }

        private async Task<IdentityResult> AssignRoleToUserAsync(AppUser user, UserRoleEnum role)
        {
            var roleName = GetUserRoleName(role);
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        private string GetUserRoleName(UserRoleEnum role)
        {
            return role switch
            {
                UserRoleEnum.Admin => "Admin",
                UserRoleEnum.Trainer => "Trainer",
                UserRoleEnum.Member => "Member",
                UserRoleEnum.Receptionist => "Receptionist",
                _ => throw new ArgumentException("Invalid user role", nameof(role))
            };
        }

        private string BuildEmailBody(string username, string callbackUrl)
        {
            return $"<h1>Dear {username}! Welcome to ATHLETIC GYM.</h1>" +
                   $"<p>Please <a href='{callbackUrl}'>Click Here</a> to confirm your email.</p>";
        }
        #endregion
    }
}