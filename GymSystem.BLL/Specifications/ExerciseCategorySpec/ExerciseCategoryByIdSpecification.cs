using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseCategorySpec
{
    public class ExerciseCategoryByIdSpecification : BaseSpecification<ExerciseCategory>
    {
        public ExerciseCategoryByIdSpecification(int categoryId)
            : base(c => c.Id == categoryId && !c.IsDeleted)
        {
        }
    }
}