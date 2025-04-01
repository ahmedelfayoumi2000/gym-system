using GymSystem.BLL.Errors;
using GymSystem.BLL.Dtos.NutritionPlan;

using GymSystem.BLL.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{
    public class NutritionPlanController : BaseApiController
    {
        private readonly INutritionPlanRepo _nutritionPlanRepo;

        public NutritionPlanController(INutritionPlanRepo nutritionPlanRepo)
        {
            _nutritionPlanRepo = nutritionPlanRepo;
        }


        [HttpGet("GetNutritionPlans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNutritionPlans()
        {
            try
            {
                var nutritionPlans = await _nutritionPlanRepo.GetNutritionPlans();
                return Ok(nutritionPlans);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(500, "An error occurred: " + ex.Message));
            }
        }


        [HttpGet("GetNutritionPlan/{nutritionPlanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNutritionPlan(int nutritionPlanId)
        {
            try
            {
                var nutritionPlan = await _nutritionPlanRepo.GetNutritionPlan(nutritionPlanId);

                if (nutritionPlan == null)
                {
                    return NotFound(new ApiResponse(404, "Nutrition Plan not found"));
                }

                return Ok(nutritionPlan);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(500, "An error occurred: " + ex.Message));
            }
        }


        [Authorize(Roles = "Admin,Trainer")]
        [HttpPost("CreateNutritionPlan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateNutritionPlan([FromForm] NutritionPlanDto nutritionPlanDto)
        {
            try
            {
                var response = await _nutritionPlanRepo.CreateNutritionPlan(nutritionPlanDto);

                if (response.StatusCode == 400)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(500, "An error occurred: " + ex.Message));
            }
        }


        [Authorize(Roles = "Admin,Trainer")]
        [HttpPut("UpdateNutritionPlan/{nutritionPlanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateNutritionPlan(int nutritionPlanId, [FromForm] NutritionPlanDto nutritionPlanDto)
        {
            try
            {
                var response = await _nutritionPlanRepo.UpdateNutritionPlan(nutritionPlanId, nutritionPlanDto);

                if (response.StatusCode == 404)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(500, "An error occurred: " + ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Trainer")]
        [HttpDelete("DeleteNutritionPlan/{nutritionPlanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteNutritionPlan(int nutritionPlanId)
        {
            try
            {
                var response = await _nutritionPlanRepo.DeleteNutritionPlan(nutritionPlanId);

                if (response.StatusCode == 404)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(500, "An error occurred: " + ex.Message));
            }
        }
    }
}
