using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlanByDayAndUserSpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlanByDayAndUserSpecification(DayOfWeek day, string userId)
            : base(w => !w.IsDeleted && w.DayOfWeek == day &&
                        (string.IsNullOrEmpty(w.TrainerId) || w.TrainerId == userId))
        {
        }
    }
}