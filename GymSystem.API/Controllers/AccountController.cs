using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Auth;
using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using GymSystem.BLL.Services.Auth;
namespace GymSystem.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;
        public AccountController(IAccountService accountService, IConfiguration configuration, ILogger<AccountService> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.LoginAsync(dto);

            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }



        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                _logger.LogInformation("Processing logout request.");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Logout attempt with no UserId claim in token. Claims available: {Claims}",
                        string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
                    return BadRequest(new ApiValidationErrorResponse
                    {
                        Errors = new List<string> { "UserId claim not found in the token." },
                        StatusCode = 400,
                        Message = "Invalid request data"
                    });
                }

                _logger.LogInformation("Initiating logout for UserId: {UserId}", userId);

                var response = await _accountService.LogoutAsync(userId);

                if (response.StatusCode == 200)
                {
                    _logger.LogInformation("User with ID {UserId} logged out successfully.", userId);
                    return Ok(response);
                }

                _logger.LogWarning("Logout failed for UserId: {UserId}. Reason: {Message}", userId, response.Message);
                return response.StatusCode switch
                {
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    _ => StatusCode(500, new ApiExceptionResponse(500, "Unexpected logout failure", response.Message))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout attempt for UserId: {UserId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while logging out", ex.Message));
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.RegisterAsync(model, GenerateCallBackUrl);
            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ForgetPassword with Email: {Email}", request?.Email);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Bad Request, You Have Made"
                });
            }

            var result = await _accountService.ForgetPassword(request.Email);
            return result.StatusCode == 400 ? BadRequest(result) : Ok(result);
        }

        [Authorize]
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(VerifyOtp dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _accountService.VerfiyOtp(dto);
            if (result.StatusCode == 400)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }


        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.ResetPasswordAsync(dto);
            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated.");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Invalid user ID.");
            }


            var result = await _accountService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }



        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ResendConfirmationEmail with Email: {Email}", request?.Email);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Bad Request, You Have Made"
                });
            }

            var result = await _accountService.ResendConfirmationEmailAsync(request.Email, GenerateCallBackUrl);
            return result.StatusCode == 400 ? BadRequest(result) : Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmUserEmail([FromQuery] string userId, [FromQuery] string confirmationToken)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(confirmationToken))
            {
                return BadRequest(new ApiResponse(400, "User ID and confirmation token are required."));
            }

            var (succeeded, errorMessage) = await _accountService.ConfirmUserEmailAsync(userId, confirmationToken);

            if (succeeded)
            {
                return RedirectPermanent("https://www.google.com/webhp?authuser=0");
            }
            else
            {
                return BadRequest(new ApiResponse(400, errorMessage));
            }
        }

        // Helper Method
        private string GenerateCallBackUrl(string token, string userId)
        {
            var baseUrl = _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(userId);
            var callBackUrl = $"{baseUrl}/api/Account/confirm-email?userId={encodedUserId}&confirmationToken={encodedToken}";
            return callBackUrl;
        }
    }
}