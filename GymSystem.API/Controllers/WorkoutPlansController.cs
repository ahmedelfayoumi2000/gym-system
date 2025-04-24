using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    [Authorize]
    public class WorkoutPlansController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IWorkoutPlanRepo _workoutPlanRepo;

        public WorkoutPlansController(IUserService userService, IWorkoutPlanRepo workoutPlanRepo)
        {
            _userService = userService;
            _workoutPlanRepo = workoutPlanRepo;
        }

        [Authorize(Roles = "Trainer")]
        [HttpPost]
        public async Task<IActionResult> CreateWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto)
        {
            workoutPlanDto.TrainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(workoutPlanDto.TrainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(workoutPlanDto.TrainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {workoutPlanDto.TrainerId} not found in the database."));
            }
            var response = await _workoutPlanRepo.CreateWorkoutPlan(workoutPlanDto);
            return HandleApiResponse(response);
        }

        [Authorize(Roles = "Trainer")]
        [HttpPost("{workoutPlanId}/exercises/{exerciseId}/membership/{membershipId}")]
        public async Task<IActionResult> AddExerciseToWorkoutPlan(int workoutPlanId, int exerciseId, int membershipId)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(trainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {trainerId} not found in the database."));
            }
            var response = await _workoutPlanRepo.AddExerciseToWorkoutPlan(workoutPlanId, exerciseId, membershipId, trainerId);
            return HandleApiResponse(response);
        }

        [Authorize(Roles = "Trainer")]
        [HttpDelete("{workoutPlanId}/exercises/{exerciseId}")]
        public async Task<IActionResult> RemoveExerciseFromWorkoutPlan(int workoutPlanId, int exerciseId, [FromQuery] int membershipId)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }
            var user = await _userService.FindByIdAsync(trainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {trainerId} not found in the database."));
            }
            var response = await _workoutPlanRepo.RemoveExerciseFromWorkoutPlan(workoutPlanId, exerciseId, membershipId, trainerId);

            return HandleApiResponse(response);
        }

        [Authorize(Roles = "Trainer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkoutPlan(int id, [FromBody] WorkoutPlanDto workoutPlanDto)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(trainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {trainerId} not found in the database."));
            }
            var response = await _workoutPlanRepo.UpdateWorkoutPlan(id, workoutPlanDto, trainerId);
            return HandleApiResponse(response);
        }

        [Authorize(Roles = "Trainer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkoutPlan(int id)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(trainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {trainerId} not found in the database."));
            }
            var response = await _workoutPlanRepo.DeleteWorkoutPlan(id, trainerId);
            return HandleApiResponse(response);
        }

        [Authorize(Roles = "Trainer")]
        [HttpGet("member/{membershipId}")]
        public async Task<IActionResult> GetWorkoutPlansForMember(int membershipId)
        {
            var trainerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(trainerId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(trainerId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {trainerId} not found in the database."));
            }
            var workoutPlans = await _workoutPlanRepo.GetWorkoutPlansForMember(membershipId, trainerId);
            return Ok(new ApiResponse(200, "Workout plans retrieved successfully", workoutPlans));
        }

        [Authorize(Roles = "Member")]
        [HttpGet("my-plans")]
        public async Task<IActionResult> GetMyWorkoutPlans()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiExceptionResponse(401, "User is not authenticated. Please log in."));
            }

            var user = await _userService.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiExceptionResponse(404, $"User with ID {userId} not found in the database."));
            }
            var workoutPlans = await _workoutPlanRepo.GetMemberWorkoutPlans(userId);
            return Ok(new ApiResponse(200, "Your workout plans retrieved successfully", workoutPlans));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkoutPlanById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Membership ID must be a positive integer."));
            }

            var response = await _workoutPlanRepo.GetWorkoutPlanById(id);
            return HandleApiResponse(response);

        }

        #region Helper Methods
        private bool IsValidId(int id) => id > 0;

        private ApiValidationErrorResponse CreateValidationError(string message)
        {
            return new ApiValidationErrorResponse
            {
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                StatusCode = 400,
                Message = message
            };
        }
        private IActionResult HandleApiResponse(ApiResponse response)
        {
            return response.StatusCode switch
            {
                200 => Ok(response),
                201 => StatusCode(201, response),
                400 => BadRequest(response),
                403 => Forbid(),
                404 => NotFound(response),
                500 => StatusCode(500, response),
                _ => StatusCode(500, response)
            };
        }
        #endregion
    }
}