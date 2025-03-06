using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    /// <summary>
    /// الوصفات الصحية
    /// </summary>

    [Authorize]
    public class RecipeController : BaseApiController
    {
        private readonly IRecipeRepo _recipeRepo;

        public RecipeController(IRecipeRepo recipeRepo)
        {
            _recipeRepo = recipeRepo ?? throw new ArgumentNullException(nameof(recipeRepo));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecipes()
        {
            try
            {
                var recipes = await _recipeRepo.GetRecipesAsync();
                return Ok(new ApiResponse(200, "Recipes retrieved successfully", recipes));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving recipes", ex.Message));
            }
        }

     
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Recipe ID must be a positive integer."));
            }

            try
            {
                var recipe = await _recipeRepo.GetRecipeAsync(id);
                if (recipe == null)
                {
                    return NotFound(new ApiResponse(404, $"Recipe with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Recipe retrieved successfully", recipe));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving recipe with ID {id}", ex.Message));
            }
        }

        #region Private Helper Methods

        private bool IsValidId(int id) => id > 0;

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