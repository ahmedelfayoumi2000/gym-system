using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealsCategorySpec
{
    public class MealsCategoryByIdSpecification : BaseSpecification<MealsCategory>
    {
        public MealsCategoryByIdSpecification(int mealsCategoryId)
            : base(mc => mc.Id == mealsCategoryId && !mc.IsDeleted)
        {
        }
    }
}