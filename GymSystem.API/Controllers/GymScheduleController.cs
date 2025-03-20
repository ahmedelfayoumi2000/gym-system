using GymSystem.BLL.Dtos.GymSchedule;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    public class GymScheduleController : BaseApiController
    {
        private readonly IGymScheduleRepo _scheduleRepo;

        public GymScheduleController(IGymScheduleRepo scheduleRepo)
        {
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));
        }

        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSchedules()
        {
            try
            {
                var response = await _scheduleRepo.GetSchedules();
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving schedules", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            try
            {
                var response = await _scheduleRepo.GetSchedule(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving schedule with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet("by-day/{dayOfWeek}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSchedulesByDay(string dayOfWeek)
        {
            try
            {
                var response = await _scheduleRepo.GetSchedulesByDay(dayOfWeek);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving schedules for day {dayOfWeek}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSchedule([FromBody] GymScheduleDto scheduleDto)
        {
            if (scheduleDto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(CreateValidationError("Invalid schedule data: " + string.Join(", ", errors)));
            }

            try
            {
                var response = await _scheduleRepo.AddSchedule(scheduleDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the schedule", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] GymScheduleDto scheduleDto)
        {
            if (scheduleDto == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(CreateValidationError("Invalid schedule data: " + string.Join(", ", errors)));
            }

            try
            {
                var response = await _scheduleRepo.UpdateSchedule(id, scheduleDto);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating schedule with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Receptionist")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var response = await _scheduleRepo.DeleteSchedule(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting schedule with ID {id}", ex.Message));
            }
        }

        #region Private Helper Methods

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = new List<string> { message },
                StatusCode = 400,
                Message = "Invalid request data"
            };
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
        #endregion
    }
}