using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlanByDayAndUserSpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlanByDayAndUserSpecification(DayOfWeek day, int membershipId)
            : base(w => !w.IsDeleted && w.DayOfWeek == day && w.MembershipId == membershipId)
        {
            AddIncludes(w => w.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);
        }
    }
}