using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Enums.Business;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class GymSchedulesByDaySpecification : BaseSpecification<GymSchedule>
    {
        public GymSchedulesByDaySpecification(DayOfWeekEnum day)
            : base(s => s.IsActive && s.DaysOfWeek.Any(d => d.DayOfWeek == day))
        {
            AddIncludes(s => s.DaysOfWeek);
        }
    }
}