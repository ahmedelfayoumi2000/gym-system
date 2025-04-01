using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.UserFavoriteExerciseSpec
{
    public class UserFavoriteExerciseByUserSpecification : BaseSpecification<UserFavoriteExercise>
    {
        public UserFavoriteExerciseByUserSpecification(string userId)
            : base(f => f.UserId == userId && !f.IsDeleted)
        {
            AddIncludes(f => f.Exercise);
        }
    }
}