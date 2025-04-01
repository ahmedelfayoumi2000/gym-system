using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.UserFavoriteExerciseSpec
{
    public class UserFavoriteExerciseByUserAndExerciseSpecification : BaseSpecification<UserFavoriteExercise>
    {
        public UserFavoriteExerciseByUserAndExerciseSpecification(string userId, int exerciseId)
            : base(f => f.UserId == userId && f.ExerciseId == exerciseId && !f.IsDeleted)
        {
        }
    }
}