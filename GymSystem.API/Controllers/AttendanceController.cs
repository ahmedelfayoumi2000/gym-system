using GymSystem.BLL.Dtos.Attendance;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security;
using System.Threading.Tasks;
using GymSystem.API.Helpers;
using GymSystem.BLL.Specifications.AttendanceSpec;
using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using AutoMapper;

namespace GymSystem.API.Controllers
{

    public class AttendanceController : BaseApiController
    {
        private readonly IDailyAttendanceRepo _attendanceRepo;
        private readonly IMapper _mapper;
        public AttendanceController(IDailyAttendanceRepo attendanceRepo, IMapper mapper)
        {
            _attendanceRepo = attendanceRepo;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpGet("getattendances")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendances([FromQuery] SpecPrams attendanceParams)
        {
            if (string.IsNullOrWhiteSpace(attendanceParams.UserCode))
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "UserCode is required." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var spec = new AttendanceByUserCodeSpec(attendanceParams);

                var countSpec = new AttendanceWithFiltersForCountSpecification(attendanceParams);

                var totalItems = await _attendanceRepo.GetCountAsync(countSpec);

                var attendances = await _attendanceRepo.GetAllWithSpecAsync(spec);

                if (attendances == null || !attendances.Any())
                {
                    return NotFound(new ApiResponse(404, "No attendance records found for the specified user."));
                }

                var data = _mapper.Map<IReadOnlyList<Attendance>, IReadOnlyList<AttendanceDto>>(attendances);

                return Ok(new Pagination<AttendanceDto>(
                    attendanceParams.PageIndex,
                    attendanceParams.PageSize,
                    totalItems,
                    data
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving attendances", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAttendance([FromBody] AttendanceDto attendanceDto)
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
                return BadRequest(new ApiExceptionResponse(500, "An error occurred while adding attendance", ex.Message));
            }
        }


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
                return BadRequest(new ApiExceptionResponse(500, "An error occurred while deleting attendance", ex.Message));
            }
        }

        [HttpGet("generate-qr")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Receptionist,Member")]
        public async Task<IActionResult> GenerateQRCodeAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new ApiResponse(401, "User authentication required."));
                }

                var qrCodeDto = await _attendanceRepo.GenerateQRCodeAsync(userId);
                return Ok(new ApiResponse(200, "QR Code generated successfully", qrCodeDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }
            catch (SecurityException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse(403, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while generating the QR Code.", ex.Message));
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
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
     
    }
}