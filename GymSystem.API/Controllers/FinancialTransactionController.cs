using GymSystem.BLL.Dtos.Payment;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;
using System.Security.Claims;
namespace GymSystem.API.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class FinancialTransactionController : BaseApiController
    {
        private readonly IFinancialTransactionService _financialTransactionService;
        public FinancialTransactionController(
            IFinancialTransactionService financialTransactionService)
        {
            _financialTransactionService = financialTransactionService ?? throw new ArgumentNullException(nameof(financialTransactionService));
        }

        [HttpPost("close-drawer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CloseDrawerAsync([FromBody] CloseDrawerRequestDto requestDto)
        {
            try
            {

                if (requestDto == null)
                {
                    return BadRequest(new ApiResponse(400, "Request data cannot be null."));
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Unauthorized(new ApiResponse(401, "User authentication required."));
                }

                var response = await _financialTransactionService.CloseDrawerAsync(requestDto, currentUserId);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (SecurityException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(403, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred during close drawer.", ex.Message));
            }
        }
    }
}