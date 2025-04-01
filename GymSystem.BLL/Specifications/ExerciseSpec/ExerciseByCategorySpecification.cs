using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseSpec
{
    public class ExerciseByCategorySpecification : BaseSpecification<Exercise>
    {
        public ExerciseByCategorySpecification(int categoryId)
            : base(e => !e.IsDeleted && e.ExerciseCategoryId == categoryId)
        {
        }
    }
}