using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.NutritionPlanSpec
{
    public class NutritionPlanByNameSpecification : BaseSpecification<NutritionPlan>
    {
        public NutritionPlanByNameSpecification(string planName)
            : base(x => x.PlanName == planName && !x.IsDeleted)
        {
        }
    }
}