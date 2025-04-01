using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MealSpec
{
    public class AllMealsSpecification : BaseSpecification<Meal>
    {
        public AllMealsSpecification()
            : base(m => !m.IsDeleted)
        {
        }
    }
}   