using GymSystem.BLL.Dtos;
using GymSystem.BLL.Dtos.plan;
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

       
        //[Authorize(Roles = "Admin")]
        //[HttpGet("filtered")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetFilteredPlans([FromQuery] SpecPrams specParams)
        //{
        //    try
        //    {
        //        var plans = await _planRepo.GetAllAsync(specParams);
        //        if (plans == null || !plans.Any())
        //        {
        //            return Ok(new ApiResponse(200, "No plans found with the specified filters.", plans));
        //        }

        //        return Ok(new ApiResponse(200, "Plans retrieved successfully", plans));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            new ApiExceptionResponse(500, "An error occurred while retrieving plans", ex.Message));
        //    }
        //}

       
        [Authorize(Roles = "Admin,Receptionist,Trainer")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPlans()
        {
            try
            {
                var response = await _planRepo.GetPlans();
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, "An error occurred while retrieving plans.", ex.Message));
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
            try
            {
                var response = await _planRepo.CreateAsync(planDto);
                return HandleApiResponse(response, StatusCodes.Status201Created);
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

            try
            {
                var response = await _planRepo.UpdateAsync(id, planDto);
                return HandleApiResponse(response);
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
            try
            {
                var response = await _planRepo.DeleteAsync(id);
                return HandleApiResponse(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiExceptionResponse(500, $"An error occurred while deleting plan with ID {id}", ex.Message));
            }
        }

        // Helper Method

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
    }
}