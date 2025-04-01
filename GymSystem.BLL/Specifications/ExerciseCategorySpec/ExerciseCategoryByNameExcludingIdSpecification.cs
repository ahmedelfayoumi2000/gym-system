using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseCategorySpec
{
    public class ExerciseCategoryByNameExcludingIdSpecification : BaseSpecification<ExerciseCategory>
    {
        public ExerciseCategoryByNameExcludingIdSpecification(string categoryName, int excludeId)
            : base(c => c.CategoryName == categoryName && c.Id != excludeId && !c.IsDeleted)
        {
        }
    }
}