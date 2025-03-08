using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{


    //الحضور اليومي
    public class AttendanceController : BaseApiController
    {
        private readonly IDailyAttendanceRepo _attendanceRepo;

        public AttendanceController(IDailyAttendanceRepo attendanceRepo)
        {
            _attendanceRepo = attendanceRepo ?? throw new ArgumentNullException(nameof(attendanceRepo));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("getattendances")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendances([FromQuery] string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode) || !ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .Concat(new[] { "UserCode is required." }).ToList(),
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var attendances = await _attendanceRepo.GetAttendancesForUserAsync(userCode);
                if (attendances == null || !attendances.Any())
                {
                    return NotFound(new ApiResponse(404, "No attendance records found for the specified user."));
                }

                return Ok(new ApiResponse(200, "Daily attendances retrieved successfully", attendances));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving daily attendances", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAttendance([FromBody] DailyAttendanceDto attendanceDto)
        {
            if (!ModelState.IsValid || attendanceDto == null)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Invalid daily attendance data"
                });
            }

            try
            {
                var response = await _attendanceRepo.AddAttendanceAsync(attendanceDto);
                return response.StatusCode switch
                {
                    201 => StatusCode(StatusCodes.Status201Created, response),
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => StatusCode(response.StatusCode ?? 500, response)
                };
            }
            catch (Exception ex)
            {
                return BadRequest( new ApiExceptionResponse(500, "An error occurred while adding daily attendance", ex.Message));
            }
        }

        /// <summary>
        /// Deletes a daily attendance record by its ID.
        /// </summary>
        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            if (id <= 0 || !ModelState.IsValid)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .Concat(new[] { "Attendance ID must be a positive integer." }).ToList(),
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var response = await _attendanceRepo.DeleteAttendanceAsync(id);
                return response.StatusCode switch
                {
                    200 => Ok(response),
                    404 => NotFound(response),
                    400 => BadRequest(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => StatusCode(response.StatusCode ?? 500, response)
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiExceptionResponse(500, "An error occurred while deleting daily attendance", ex.Message));
            }
        }

        /// <summary>
        /// Generates a QR Code for a user to use for check-in.
        /// </summary>
        [HttpGet("qrcode/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Receptionist,Member")] 
        public async Task<IActionResult> GenerateQRCode(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            try
            {
                var qrCodeDto = await _attendanceRepo.GenerateQRCodeAsync(userId);
                return Ok(new ApiResponse(200, "QR Code generated successfully", qrCodeDto));
            }
            catch (ApplicationException ex)
            {
                return NotFound(new ApiResponse(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while generating the QR Code", ex.Message));
            }
        }

        [HttpPost("checkin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Receptionist")] 
        public async Task<IActionResult> CheckIn([FromBody] AttendanceCheckInDto checkInDto)
        {
            if (!ModelState.IsValid || checkInDto == null)
            {
                return BadRequest(CreateValidationError("Invalid check-in data"));
            }

            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse(401, "User not authenticated. Please provide a valid token."));
                }

                var response = await _attendanceRepo.CheckInAsync(checkInDto, currentUserId);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while recording the check-in", ex.Message));
            }
        }

        #region Private Helper Methods

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }

        private IActionResult HandleApiResponse(ApiResponse response, int successStatusCode = StatusCodes.Status200OK)
        {
            return response.StatusCode switch
            {
                200 => Ok(response),
                201 => StatusCode(StatusCodes.Status201Created, response),
                400 => BadRequest(response),
                401 => Unauthorized(response),
                404 => NotFound(response),
                409 => Conflict(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(response.StatusCode ?? 500, response)
            };
        }

        #endregion
    }
}