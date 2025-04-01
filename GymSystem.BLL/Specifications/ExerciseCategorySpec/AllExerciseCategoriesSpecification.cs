using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseCategorySpec
{
    public class AllExerciseCategoriesSpecification : BaseSpecification<ExerciseCategory>
    {
        public AllExerciseCategoriesSpecification()
            : base(c => !c.IsDeleted)
        {
        }
    }
}