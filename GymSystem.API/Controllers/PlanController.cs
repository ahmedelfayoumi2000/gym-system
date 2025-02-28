using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymSystem.API.Controllers
{

    //إضافة خطة
    public class PlanController : BaseApiController
    {
        private readonly IPlanRepo _planRepo;

        public PlanController(IPlanRepo planRepo)
        {
            _planRepo = planRepo ?? throw new ArgumentNullException(nameof(planRepo));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPlans([FromQuery] SpecPrams specParams)
        {
            try
            {
                var plans = await _planRepo.GetAllAsync(specParams);
                return Ok(new ApiResponse(200, "Plans retrieved successfully", plans));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving plans", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlanById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Plan ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var plan = await _planRepo.GetByIdAsync(id);
                if (plan == null)
                {
                    return NotFound(new ApiResponse(404, $"Plan with ID {id} not found"));
                }

                return Ok(new ApiResponse(200, "Plan retrieved successfully", plan));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while retrieving plan with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePlan([FromBody] PlanDto planDto)
        {
            if (!ModelState.IsValid || planDto == null)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Invalid plan data"
                });
            }

            try
            {
                var response = await _planRepo.CreateAsync(planDto);
                return response.StatusCode switch
                {
                    201 => StatusCode(StatusCodes.Status201Created, response),
                    400 => BadRequest(response),
                    500 => StatusCode(StatusCodes.Status500InternalServerError, response),
                    _ => StatusCode(response.StatusCode ?? 500, response)
                };
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while creating the plan", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] PlanDto planDto)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Plan ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            if (!ModelState.IsValid || planDto == null)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList(),
                    StatusCode = 400,
                    Message = "Invalid plan data"
                });
            }

            try
            {
                var response = await _planRepo.UpdateAsync(id, planDto);
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while updating plan with ID {id}", ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePlan(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiValidationErrorResponse
                {
                    Errors = new List<string> { "Plan ID must be a positive integer." },
                    StatusCode = 400,
                    Message = "Invalid request data"
                });
            }

            try
            {
                var response = await _planRepo.DeleteAsync(id);
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
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting plan with ID {id}", ex.Message));
            }
        }
    }
}