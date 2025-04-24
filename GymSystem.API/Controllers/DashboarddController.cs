using GymSystem.BLL.Dtos.Dashboard;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    public class DashboarddController : BaseApiController
    {
        private readonly IDashboardRepo _dashboardRepo;

        public DashboarddController(IDashboardRepo dashboardRepo)
        {
            _dashboardRepo = dashboardRepo ?? throw new ArgumentNullException(nameof(dashboardRepo));
        }

        [Authorize]
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboardStats([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new ApiResponse(400, "User ID not found."));

                var response = await _dashboardRepo.GetDashboardStats(userId, year, month);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving dashboard stats", ex.Message));
            }
        }

        [Authorize]
        [HttpPost("add-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDailyStats([FromBody] AddDailyStatsDto statsDto)
        {
            if (statsDto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = errors,
                    StatusCode = 400,
                    Message = "Invalid stats data: " + string.Join(", ", errors)
                });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new ApiResponse(400, "User ID not found."));

                var response = await _dashboardRepo.AddDailyStats(userId, statsDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while saving daily stats", ex.Message));
            }
        }

        private IActionResult HandleApiResponse(ApiResponse response, int successStatusCode = StatusCodes.Status200OK)
        {
            return response.StatusCode switch
            {
                200 => Ok(response),
                201 => StatusCode(StatusCodes.Status201Created, response),
                400 => BadRequest(response),
                404 => NotFound(response),
                409 => Conflict(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }
    }
}