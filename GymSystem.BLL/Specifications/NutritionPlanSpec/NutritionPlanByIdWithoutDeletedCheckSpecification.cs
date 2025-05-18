using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.NutritionPlanSpec
{
    public class NutritionPlanByIdWithoutDeletedCheckSpecification : BaseSpecification<NutritionPlan>
    {
        public NutritionPlanByIdWithoutDeletedCheckSpecification(int nutritionPlanId)
            : base(x => x.Id == nutritionPlanId)
        {

        }
    }
}