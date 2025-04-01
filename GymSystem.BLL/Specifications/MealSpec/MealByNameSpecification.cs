using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealSpec
{
    public class MealByNameSpecification : BaseSpecification<Meal>
    {
        public MealByNameSpecification(string mealName)
            : base(m => m.MealName == mealName && !m.IsDeleted)
        {
        }
    }
}