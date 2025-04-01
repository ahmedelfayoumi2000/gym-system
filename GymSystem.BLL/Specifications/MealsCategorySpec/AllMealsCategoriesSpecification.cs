using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealsCategorySpec
{
    public class AllMealsCategoriesSpecification : BaseSpecification<MealsCategory>
    {
        public AllMealsCategoriesSpecification()
            : base(mc => !mc.IsDeleted)
        {
        }
    }
}