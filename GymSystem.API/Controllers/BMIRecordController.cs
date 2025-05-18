using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GymSystem.API.Controllers
{

    public class BMIRecordController : BaseApiController
    {
        private readonly IBMIRecordRepo _bMIRecordRepo;
        private readonly ILogger<BMIRecordController> _logger;

        public BMIRecordController(IBMIRecordRepo bMIRecordRepo, ILogger<BMIRecordController> logger)
        {
            _bMIRecordRepo = bMIRecordRepo ?? throw new ArgumentNullException(nameof(bMIRecordRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "Admin,Trainer,Member")]
        [HttpGet("getBMIRecordsForUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBMIRecordsForUser()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return BadRequest(new ApiResponse(400, "UserId claim not found in the token"));
                }

                var result = await _bMIRecordRepo.GetBMIRecordsForUser(userIdClaim.Value);
                return Ok(new ApiResponse(200, "BMI records retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving BMI records for user.");
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving BMI records", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPost("addBMIRecord")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddBMIRecord([FromBody] BMIRecordDto bmiRecord)
        {
            if (!ModelState.IsValid || bmiRecord == null)
            {
                return BadRequest(CreateValidationErrorResponse("Invalid BMI record data"));
            }

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    _logger.LogWarning("UserId claim not found in token for AddBMIRecord request.");
                    return BadRequest(new ApiResponse(400, "UserId claim not found in the token"));
                }

                var result = await _bMIRecordRepo.AddBMIRecord(bmiRecord);
                if (result.StatusCode == 200)
                {
                    _logger.LogInformation("BMI record added successfully for UserId: {UserId}", bmiRecord.UserId);
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while adding BMI record", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpDelete("deleteBMIRecord/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBMIRecord(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "BMI record ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var result = await _bMIRecordRepo.DeleteBMIRecord(id);

                if (result.StatusCode == 200)
                {
                    return Ok(result);
                }

                if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while deleting BMI record", ex.Message));
            }
        }

        private ApiValidationErrorResponse CreateValidationErrorResponse(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }

        private ActionResult<ApiResponse> HandleException(Exception ex)
        {
            return StatusCode(500, new ApiExceptionResponse(500, "An unexpected error occurred", ex.Message));
        }
    }
}