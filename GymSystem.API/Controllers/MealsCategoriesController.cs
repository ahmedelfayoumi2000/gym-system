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

    public class MealsCategoriesController : BaseApiController
    {
        private readonly IMealsCategoryRepo _mealsCategoryRepo;
        private readonly ILogger<MealsCategoriesController> _logger;

        public MealsCategoriesController(IMealsCategoryRepo mealsCategoryRepo, ILogger<MealsCategoriesController> logger)
        {
            _mealsCategoryRepo = mealsCategoryRepo ?? throw new ArgumentNullException(nameof(mealsCategoryRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = "Admin,Trainer,Member")]
        [HttpGet("GetAllMealsCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMealsCategories()
        {
            try
            {
                _logger.LogInformation("Fetching all active meals categories by user with role: {Roles}", User.FindFirst(ClaimTypes.Role)?.Value);
                var mealsCategories = await _mealsCategoryRepo.GetAllMealsCategory();

                _logger.LogInformation("Successfully retrieved {Count} active meals categories.", mealsCategories.Count());
                return Ok(new ApiResponse(200, "Meals categories retrieved successfully", mealsCategories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all meals categories.");
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving meals categories", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer,Member")]
        [HttpGet("GetMealsCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMealsCategory(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid meals category ID provided for GetMealsCategory: {Id}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Fetching meals category with ID: {Id} by user with role: {Roles}", id, User.FindFirst(ClaimTypes.Role)?.Value);
                var mealsCategory = await _mealsCategoryRepo.GetMealsCategoryById(id);

                if (mealsCategory == null)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found.", id);
                    return NotFound(new ApiResponse(404, $"Meals category with ID {id} not found"));
                }

                _logger.LogInformation("Meals category with ID {Id} retrieved successfully.", id);
                return Ok(new ApiResponse(200, "Meals category retrieved successfully", mealsCategory));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meals category with ID: {Id}", id);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while retrieving the meals category", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddMealsCategory([FromBody] MealsCategoryDto mealsCategory)
        {
            if (!ModelState.IsValid || mealsCategory == null)
            {
                _logger.LogWarning("Invalid model state or null data for AddMealsCategory.");
                return BadRequest(CreateValidationErrorResponse("Invalid meals category data"));
            }

            try
            {
                _logger.LogInformation("Attempting to add meals category with name: {CategoryName} by user with role: {Roles}", mealsCategory.CategoryName, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealsCategoryRepo.Add(mealsCategory);

                if (response.StatusCode == 201)
                {
                    _logger.LogInformation("Meals category {CategoryName} added successfully.", mealsCategory.CategoryName);
                    return StatusCode(StatusCodes.Status201Created, response);
                }

                _logger.LogWarning("Failed to add meals category {CategoryName}: {Message}", mealsCategory.CategoryName, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding meals category with name: {CategoryName}", mealsCategory?.CategoryName);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while adding the meals category", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMealsCategory([FromBody] MealsCategoryDto mealsCategory)
        {
            if (!ModelState.IsValid || mealsCategory == null)
            {
                _logger.LogWarning("Invalid model state or null data for UpdateMealsCategory.");
                return BadRequest(CreateValidationErrorResponse("Invalid meals category data"));
            }

            if (mealsCategory.MealsCategoryId <= 0)
            {
                _logger.LogWarning("Invalid meals category ID provided for UpdateMealsCategory: {Id}", mealsCategory.MealsCategoryId);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to update meals category with ID: {Id} by user with role: {Roles}", mealsCategory.MealsCategoryId, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealsCategoryRepo.Update(mealsCategory);

                if (response.StatusCode == 200)
                {
                    _logger.LogInformation("Meals category with ID {Id} updated successfully.", mealsCategory.MealsCategoryId);
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found.", mealsCategory.MealsCategoryId);
                    return NotFound(response);
                }

                _logger.LogWarning("Failed to update meals category with ID {Id}: {Message}", mealsCategory.MealsCategoryId, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meals category with ID: {Id}", mealsCategory?.MealsCategoryId);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while updating the meals category", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMealsCategory(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid meals category ID provided for DeleteMealsCategory: {Id}", id);
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                _logger.LogInformation("Attempting to delete meals category with ID: {Id} by user with role: {Roles}", id, User.FindFirst(ClaimTypes.Role)?.Value);
                var response = await _mealsCategoryRepo.Delete(id);

                if (response.StatusCode == 200)
                {
                    _logger.LogInformation("Meals category with ID {Id} deleted successfully.", id);
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    _logger.LogWarning("Meals category with ID {Id} not found.", id);
                    return NotFound(response);
                }

                _logger.LogWarning("Failed to delete meals category with ID {Id}: {Message}", id, response.Message);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting meals category with ID: {Id}", id);
                return StatusCode(500, new ApiExceptionResponse(500, "An error occurred while deleting the meals category", ex.Message));
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