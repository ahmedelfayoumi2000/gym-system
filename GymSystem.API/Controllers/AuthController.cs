using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Auth;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{
    public class AuthController : BaseApiController
    {

        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;


        public AuthController(ITokenService tokenService, ILogger<AuthController> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            if (!ModelState.IsValid || request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Invalid or missing refresh token in RefreshToken request.");
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                                     .Append(string.IsNullOrWhiteSpace(request?.RefreshToken) ? "Refresh token is required." : null)
                                     .Where(e => e != null)
                                     .ToList(),
                    StatusCode = 400,
                    Message = "Invalid token request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to refresh token with provided refresh token.");
                var (newJwtToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(request.RefreshToken);

                if (string.IsNullOrWhiteSpace(newJwtToken) || newRefreshToken == null || string.IsNullOrWhiteSpace(newRefreshToken.Token))
                {
                    return StatusCode(500, new ApiExceptionResponse(500, "Token refresh failed due to internal error."));
                }

                _logger.LogInformation("Token refreshed successfully for refresh token.");
                return Ok(new TokenResponse
                {
                    Token = newJwtToken,
                    RefreshToken = newRefreshToken.Token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access attempt to refresh token: {Message}", ex.Message);
                return Unauthorized(new ApiResponse(401, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while refreshing the token", ex.Message));
            }
        }


        [Authorize]
        [HttpPost("revoke-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            if (!ModelState.IsValid || request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Invalid or missing refresh token in RevokeToken request.");
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                                     .Append(string.IsNullOrWhiteSpace(request?.RefreshToken) ? "Refresh token is required." : null)
                                     .Where(e => e != null)
                                     .ToList(),
                    StatusCode = 400,
                    Message = "Invalid token revocation request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to revoke refresh token.");
                var success = await _tokenService.RevokeTokenAsync(request.RefreshToken);

                if (!success)
                {
                    _logger.LogWarning("Failed to revoke token: Invalid or already revoked token provided.");
                    return BadRequest(new ApiResponse(400, "Invalid or already revoked token."));
                }

                _logger.LogInformation("Refresh token revoked successfully.");
                return Ok(new ApiResponse(200, "Token revoked successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt to revoke token: {Message}", ex.Message);
                return Unauthorized(new ApiResponse(401, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token.");
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while revoking the token", ex.Message));
            }
        }
    }
}
