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

    public class MealController : BaseApiController
    {
        private readonly IMealRepo _mealRepo;
        private readonly ILogger<MealController> _logger;

        public MealController(IMealRepo mealRepo, ILogger<MealController> logger)
        {
            _mealRepo = mealRepo ?? throw new ArgumentNullException(nameof(mealRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "Admin,Trainer,Member")]
        [HttpGet("GetAllMeals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMeals()
        {
            try
            {
                _logger.LogInformation("Fetching all active meals by user with role: {Roles}", User.FindFirst(ClaimTypes.Role)?.Value);
                var meals = await _mealRepo.GetAllMeals();

                _logger.LogInformation("Successfully retrieved {Count} active meals.", meals.Count());
                return Ok(new ApiResponse(200, "Meals retrieved successfully", meals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all meals.");
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving meals", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer,Member")]
        [HttpGet("GetMealById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMealById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid meal ID provided for GetMealById: {Id}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meal ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Fetching meal with ID: {Id} by user with role: {Roles}", id, User.FindFirst(ClaimTypes.Role)?.Value);
                var meal = await _mealRepo.GetMealById(id);

                if (meal == null)
                {
                    _logger.LogWarning("Meal with ID {Id} not found.", id);
                    return NotFound(new ApiResponse(404, $"Meal with ID {id} not found"));
                }

                _logger.LogInformation("Meal with ID {Id} retrieved successfully.", id);
                return Ok(new ApiResponse(200, "Meal retrieved successfully", meal));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meal with ID: {Id}", id);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving the meal", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateMeal([FromBody] MealDto meal)
        {
            if (!ModelState.IsValid || meal == null)
            {
                _logger.LogWarning("Invalid model state or null data for CreateMeal.");
                return BadRequest(CreateValidationErrorResponse("Invalid meal data"));
            }

            try
            {
                _logger.LogInformation("Attempting to create meal with name: {MealName} by user with role: {Roles}", meal.MealName, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealRepo.CreateMeal(meal);

                if (response.StatusCode == 201)
                {
                    _logger.LogInformation("Meal {MealName} created successfully.", meal.MealName);
                    return StatusCode(StatusCodes.Status201Created, response);
                }

                _logger.LogWarning("Failed to create meal {MealName}: {Message}", meal.MealName, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meal with name: {MealName}", meal?.MealName);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while creating the meal", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMeal(int id, [FromBody] MealDto meal)
        {
            if (!ModelState.IsValid || meal == null)
            {
                _logger.LogWarning("Invalid model state or null data for UpdateMeal with ID: {Id}", id);
                return BadRequest(CreateValidationErrorResponse("Invalid meal data"));
            }

            if (id <= 0)
            {
                _logger.LogWarning("Invalid meal ID provided for UpdateMeal: {Id}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meal ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to update meal with ID: {Id} by user with role: {Roles}", id, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealRepo.UpdateMeal(id, meal);

                if (response.StatusCode == 200)
                {
                    _logger.LogInformation("Meal with ID {Id} updated successfully.", id);
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    _logger.LogWarning("Meal with ID {Id} not found.", id);
                    return NotFound(response);
                }

                _logger.LogWarning("Failed to update meal with ID {Id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meal with ID: {Id}", id);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while updating the meal", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid meal ID provided for DeleteMeal: {Id}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meal ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to delete meal with ID: {Id} by user with role: {Roles}", id, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealRepo.DeleteMeal(id);

                if (response.StatusCode == 200)
                {
                    _logger.LogInformation("Meal with ID {Id} deleted successfully.", id);
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    _logger.LogWarning("Meal with ID {Id} not found.", id);
                    return NotFound(response);
                }

                _logger.LogWarning("Failed to delete meal with ID {Id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meal with ID: {Id}", id);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while deleting the meal", ex.Message));
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