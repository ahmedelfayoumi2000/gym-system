using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlansByDaySpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlansByDaySpecification(DayOfWeek dayOfWeek)
            : base(w => !w.IsDeleted && w.DayOfWeek == dayOfWeek)
        {
            AddIncludes(w => w.Exercises);
        }
    }
}