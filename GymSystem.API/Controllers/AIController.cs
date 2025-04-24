using GymSystem.BLL.Dtos.NutritionPlan;
using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymSystem.API.Controllers
{
  
    public class AIController : BaseApiController
    {
        private readonly IExerciseRepo _exerciseRepo;
        private readonly INutritionPlanRepo _nutritionPlanRepo;

        public AIController(IExerciseRepo exerciseRepo, INutritionPlanRepo nutritionPlanRepo)
        {
            _exerciseRepo = exerciseRepo;
            _nutritionPlanRepo = nutritionPlanRepo;
        }

        [HttpPost("generate-plan/{userId}")]
        public async Task<ActionResult> GeneratePlanForUser([FromRoute] int? userId)
        {
            try
            {
                if (!userId.HasValue)
                {
                    return BadRequest(new ApiResponse(400, "User ID is required and must be a valid integer."));
                }

                var exerciseResponse = await _exerciseRepo.GenerateExercisesForUserAsync(userId.Value);
                if (!exerciseResponse.StatusCode.HasValue || exerciseResponse.StatusCode.Value != 201)
                {
                    return StatusCode(exerciseResponse.StatusCode ?? 500, exerciseResponse);
                }

                var nutritionResponse = await _nutritionPlanRepo.GenerateNutritionPlanForUserAsync(userId.Value);
                if (!nutritionResponse.StatusCode.HasValue || nutritionResponse.StatusCode.Value != 201)
                {
                    return StatusCode(nutritionResponse.StatusCode ?? 500, nutritionResponse);
                }

                var exercises = exerciseResponse.Data as List<ExerciseDto>;
                var nutritionPlan = nutritionResponse.Data as NutritionPlanDto;

                if (exercises == null || nutritionPlan == null)
                {
                    return StatusCode(500, new ApiResponse(500, "Failed to cast response data to the expected types."));
                }

                return Ok(new { Exercises = exercises, NutritionPlan = nutritionPlan });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, $"Internal server error: {ex.Message}"));
            }
        }


    }
}
