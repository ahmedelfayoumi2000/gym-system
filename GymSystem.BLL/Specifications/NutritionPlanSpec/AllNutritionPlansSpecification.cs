using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.NutritionPlanSpec
{
    public class AllNutritionPlansSpecification : BaseSpecification<NutritionPlan>
    {
        public AllNutritionPlansSpecification()
            : base(x => !x.IsDeleted)
        {
        }
    }
}