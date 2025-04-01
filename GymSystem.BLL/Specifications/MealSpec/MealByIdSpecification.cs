using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealSpec
{
    public class MealByIdSpecification : BaseSpecification<Meal>
    {
        public MealByIdSpecification(int mealId)
            : base(m => m.Id == mealId && !m.IsDeleted)
        {
        }
    }
}