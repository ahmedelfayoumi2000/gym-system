using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.NutritionPlanSpec
{
    public class NutritionPlanByIdSpecification : BaseSpecification<NutritionPlan>
    {
        public NutritionPlanByIdSpecification(int nutritionPlanId)
            : base(x => x.Id == nutritionPlanId && !x.IsDeleted)
        {
        }
    }
}