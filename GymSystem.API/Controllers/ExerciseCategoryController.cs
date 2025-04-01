using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{

    public class ExerciseCategoryController : BaseApiController
    {
        private readonly IExerciseCategoryRepo _exerciseCategoryRepo;

        public ExerciseCategoryController(IExerciseCategoryRepo exerciseCategoryRepo)
        {
            _exerciseCategoryRepo = exerciseCategoryRepo ?? throw new ArgumentNullException(nameof(exerciseCategoryRepo));
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddExerciseCategory([FromForm] ExerciseCategoryDto exerciseCategoryDto)
        {
            try
            {
                var response = await _exerciseCategoryRepo.AddExerciseCategory(exerciseCategoryDto);
                if (response.StatusCode == 201)
                {
                    return CreatedAtAction(nameof(GetExerciseCategory), new { id = (response.Data as ExerciseCategoryDto)?.Id }, response);
                }
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while adding the exercise category.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteExerciseCategory(int id)
        {
            try
            {
                var response = await _exerciseCategoryRepo.DeleteExerciseCategory(id);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while deleting the exercise category.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExerciseCategory(int id)
        {
            try
            {
                var category = await _exerciseCategoryRepo.GetExerciseCategory(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while retrieving the exercise category.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExerciseCategories()
        {
            try
            {
                var categories = await _exerciseCategoryRepo.GetExerciseCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while retrieving exercise categories.", ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateExerciseCategory(int id, [FromForm] ExerciseCategoryDto exerciseCategoryDto)
        {
            try
            {
                var response = await _exerciseCategoryRepo.UpdateExerciseCategory(id, exerciseCategoryDto);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiExceptionResponse(500, "An unexpected error occurred while updating the exercise category.", ex.Message));
            }
        }
    }
}