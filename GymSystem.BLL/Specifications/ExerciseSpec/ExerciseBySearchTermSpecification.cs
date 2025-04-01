using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ExerciseSpec
{
    public class ExerciseBySearchTermSpecification : BaseSpecification<Exercise>
    {
        public ExerciseBySearchTermSpecification(string searchTerm)
            : base(e => !e.IsDeleted &&
                        (e.ExerciseName.ToLower().Contains(searchTerm.ToLower()) ||
                         e.Description.ToLower().Contains(searchTerm.ToLower())))
        {
        }
    }
}