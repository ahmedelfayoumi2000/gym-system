using GymSystem.BLL.Errors;
using GymSystem.BLL.Dtos.NutritionPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface INutritionPlanRepo
    {
        Task<ApiResponse> GenerateNutritionPlanForUserAsync(string userId, AINutritionPlan aiNutritionPlan);
        Task<ApiResponse> CreateNutritionPlan(NutritionPlanDto nutritionPlanDto);
        Task<ApiResponse> UpdateNutritionPlan(int nutritionPlanId, NutritionPlanDto nutritionPlanDto);
        Task<ApiResponse> DeleteNutritionPlan(int nutritionPlanId);
        Task<NutritionPlanDto> GetNutritionPlan(int nutritionPlanId);
        Task<IEnumerable<NutritionPlanDto>> GetNutritionPlans();
    }
}
