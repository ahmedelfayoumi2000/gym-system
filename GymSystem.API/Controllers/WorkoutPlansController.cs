using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    [Authorize] 
    public class WorkoutPlansController : BaseApiController
    {
        private readonly IWorkoutPlanRepo _workoutPlanRepo;

        public WorkoutPlansController(IWorkoutPlanRepo workoutPlanRepo)
        {
            _workoutPlanRepo = workoutPlanRepo ?? throw new ArgumentNullException(nameof(workoutPlanRepo));
        }

        // POST: api/WorkoutPlans
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto)
        {
            if (workoutPlanDto == null)
            {
                return BadRequest(new ApiResponse(400, "Workout plan data cannot be null."));
            }

            var response = await _workoutPlanRepo.CreateWorkoutPlan(workoutPlanDto);
            return HandleApiResponse(response);
        }

        // PUT: api/WorkoutPlans/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWorkoutPlan(int id, [FromBody] WorkoutPlanDto workoutPlanDto)
        {
            if (id <= 0 || workoutPlanDto == null)
            {
                return BadRequest(new ApiResponse(400, "Invalid workout plan ID or data."));
            }

            var response = await _workoutPlanRepo.UpdateWorkoutPlan(id, workoutPlanDto);
            return HandleApiResponse(response);
        }

        // DELETE: api/WorkoutPlans/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWorkoutPlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse(400, "Invalid workout plan ID."));
            }

            var response = await _workoutPlanRepo.DeleteWorkoutPlan(id);
            return HandleApiResponse(response);
        }

        // GET: api/WorkoutPlans/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkoutPlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse(400, "Invalid workout plan ID."));
            }

            var workoutPlan = await _workoutPlanRepo.GetWorkoutPlan(id);
            if (workoutPlan == null)
            {
                return NotFound(new ApiResponse(404, $"Workout plan with ID {id} not found."));
            }

            return Ok(new ApiResponse(200, "Workout plan retrieved successfully", workoutPlan));
        }

        // GET: api/WorkoutPlans
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkoutPlans()
        {
            try
            {
                var workoutPlans = await _workoutPlanRepo.GetWorkoutPlans();
                return Ok(new ApiResponse(200, "Workout plans retrieved successfully", workoutPlans));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "Error retrieving workout plans", ex.Message));
            }
        }

        // GET: api/WorkoutPlans/day/{dayOfWeek}
        [HttpGet("day/{dayOfWeek}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkoutPlansByDay(string dayOfWeek)
        {
            if (!Enum.TryParse<DayOfWeek>(dayOfWeek, true, out var day))
            {
                return BadRequest(new ApiResponse(400, "Invalid day of week."));
            }

            try
            {
                var workoutPlans = await _workoutPlanRepo.GetWorkoutPlansByDay(day);
                return Ok(new ApiResponse(200, "Workout plans retrieved successfully", workoutPlans));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"Error retrieving workout plans for day {dayOfWeek}", ex.Message));
            }
        }

        private IActionResult HandleApiResponse(ApiResponse response)
        {
            return response.StatusCode switch
            {
                200 => Ok(response),
                201 => StatusCode(201, response),
                400 => BadRequest(response),
                404 => NotFound(response),
                500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}