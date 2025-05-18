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
                var mealsCategories = await _mealsCategoryRepo.GetAllMealsCategory();

                return Ok(new ApiResponse(200, "Meals categories retrieved successfully", mealsCategories));
            }
            catch (Exception ex)
            {
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
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var mealsCategory = await _mealsCategoryRepo.GetMealsCategoryById(id);

                if (mealsCategory == null)
                {
                    return NotFound(new ApiResponse(404, $"Meals category with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Meals category retrieved successfully", mealsCategory));
            }
            catch (Exception ex)
            {
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
                return BadRequest(CreateValidationErrorResponse("Invalid meals category data"));
            }

            try
            {
                var response = await _mealsCategoryRepo.Add(mealsCategory);

                if (response.StatusCode == 201)
                {
                    return StatusCode(StatusCodes.Status201Created, response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
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
                return BadRequest(CreateValidationErrorResponse("Invalid meals category data"));
            }

            if (mealsCategory.MealsCategoryId <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var response = await _mealsCategoryRepo.Update(mealsCategory);

                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    return NotFound(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
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
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Meals category ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var response = await _mealsCategoryRepo.Delete(id);

                if (response.StatusCode == 200)
                {
                    return Ok(response);
                }

                if (response.StatusCode == 404)
                {
                    return NotFound(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
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