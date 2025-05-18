using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.UserFavoriteExerciseSpec
{
    public class UserFavoriteExerciseByUserAndExerciseSpecification : BaseSpecification<UserFavoriteExercise>
    {
        public UserFavoriteExerciseByUserAndExerciseSpecification(string userId)
            : base(f => f.UserId == userId && !f.IsDeleted)
        {
            AddIncludes(c => c.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);

        }
    }
}