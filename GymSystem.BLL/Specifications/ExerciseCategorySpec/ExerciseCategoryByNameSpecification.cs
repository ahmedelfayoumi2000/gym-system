using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseCategorySpec
{
    public class ExerciseCategoryByNameSpecification : BaseSpecification<ExerciseCategory>
    {
        public ExerciseCategoryByNameSpecification(string categoryName, int? excludeId = null)
            : base(c => c.CategoryName == categoryName && (excludeId == null || c.Id != excludeId) && !c.IsDeleted)
        {
        }
    }
}