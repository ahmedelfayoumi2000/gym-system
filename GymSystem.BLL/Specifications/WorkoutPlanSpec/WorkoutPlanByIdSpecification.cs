using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlanByIdSpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlanByIdSpecification(int workoutPlanId)
            : base(w => w.Id == workoutPlanId && !w.IsDeleted)
        {
            AddIncludes(w => w.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);

        }
    }
}