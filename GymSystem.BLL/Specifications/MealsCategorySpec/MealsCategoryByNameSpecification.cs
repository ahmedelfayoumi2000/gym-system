using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealsCategorySpec
{
    public class MealsCategoryByNameSpecification : BaseSpecification<MealsCategory>
    {
        public MealsCategoryByNameSpecification(string categoryName)
            : base(mc => mc.CategoryName == categoryName && !mc.IsDeleted)
        {
        }
    }
}