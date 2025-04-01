using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{
    /// <summary>
    ///  التمارين اليومية
    /// </summary>
    [Authorize]
    public class ExercisesController : BaseApiController
    {
        private readonly IExerciseRepo _exerciseRepo;

        public ExercisesController(IExerciseRepo exerciseRepo)
        {
            _exerciseRepo = exerciseRepo ?? throw new ArgumentNullException(nameof(exerciseRepo));
        }

        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExercisesByCategory(int categoryId)
        {
            if (!IsValidId(categoryId))
            {
                return BadRequest(CreateValidationError("Category ID must be a positive integer."));
            }

            try
            {
                var exercises = await _exerciseRepo.GetExercisesByCategoryAsync(categoryId);
                return Ok(new ApiResponse(200, "Exercises retrieved successfully", exercises));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"Error retrieving exercises for category {categoryId}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseDto exerciseDto)
        {
            if (exerciseDto == null)
            {
                return BadRequest(new ApiResponse(400, "Exercise data cannot be null"));
            }

            try
            {
                var addedExercise = await _exerciseRepo.AddExerciseAsync(exerciseDto);
                return HandleApiResponse(new ApiResponse(201, "Exercise added successfully", addedExercise), StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "Error adding exercise", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateExercise(int id, [FromBody] ExerciseDto exerciseDto)
        {
            if (!IsValidId(id) || exerciseDto == null)
            {
                return BadRequest(new ApiResponse(400, "Invalid exercise ID or data"));
            }

            try
            {
                var updatedExercise = await _exerciseRepo.UpdateExerciseAsync(id, exerciseDto);
                return Ok(new ApiResponse(200, "Exercise updated successfully", updatedExercise));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"Error updating exercise with ID {id}", ex.Message));
            }
        }


        [HttpGet("daily")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyExercises()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var exercises = await _exerciseRepo.GetDailyExercisesAsync(userId);
                return Ok(new ApiResponse(200, "Daily exercises retrieved successfully", exercises));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving daily exercises", ex.Message));
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchExercises([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(CreateValidationError("Search term cannot be empty."));
            }

            try
            {
                var exercises = await _exerciseRepo.SearchExercisesAsync(searchTerm);
                return Ok(new ApiResponse(200, "Exercises retrieved successfully", exercises));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while searching exercises", ex.Message));
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExerciseById(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Exercise ID must be a positive integer."));
            }

            try
            {
                var exercise = await _exerciseRepo.GetExerciseAsync(id);
                if (exercise == null)
                {
                    return NotFound(new ApiResponse(404, $"Exercise with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Exercise retrieved successfully", exercise));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving exercise with ID {id}", ex.Message));
            }
        }

        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFavoriteExercises()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
                }

                var exercises = await _exerciseRepo.GetFavoritesAsync(userId);
                return Ok(new ApiResponse(200, "Favorite exercises retrieved successfully", exercises));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving favorite exercises", ex.Message));
            }
        }


        [HttpPost("{id}/favorites")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToFavorites(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Exercise ID must be a positive integer."));
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _exerciseRepo.AddToFavoritesAsync(id, userId);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while adding exercise with ID {id} to favorites", ex.Message));
            }
        }

        [HttpDelete("{id}/favorites")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFromFavorites(int id)
        {
            if (!IsValidId(id))
            {
                return BadRequest(CreateValidationError("Exercise ID must be a positive integer."));
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var response = await _exerciseRepo.RemoveFromFavoritesAsync(id, userId);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"Error removing exercise with ID {id} from favorites", ex.Message));
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