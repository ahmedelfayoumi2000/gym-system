using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseSpec
{
    public class ExerciseByIdSpecification : BaseSpecification<Exercise>
    {
        public ExerciseByIdSpecification(int id)
            : base(e => e.Id == id && !e.IsDeleted)
        {
            AddIncludes(m => m.UserFavoriteExercises);
            AddIncludes(m => m.WorkoutPlans);
            AddIncludes(m => m.ExerciseCategory);

        }
    }
}