using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class AllWorkoutPlansSpecification : BaseSpecification<WorkoutPlan>
    {
        public AllWorkoutPlansSpecification()
            : base(w => !w.IsDeleted)
        {
            AddIncludes(w => w.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);

            AddOrderBy(w => w.DayOfWeek);
        }
    }
}