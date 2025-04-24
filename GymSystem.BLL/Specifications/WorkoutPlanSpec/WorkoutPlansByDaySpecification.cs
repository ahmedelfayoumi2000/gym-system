using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlansByDaySpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlansByDaySpecification(DayOfWeek dayOfWeek, string trainerId = null, int? membershipId = null)
            : base(w => !w.IsDeleted && w.DayOfWeek == dayOfWeek
                        && (trainerId == null || w.TrainerId == trainerId)
                        && (membershipId == null || w.MembershipId == membershipId))
        {
            AddIncludes(w => w.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);
        }
    }
}