using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Enums.Business;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class GymSchedulesByDaySpecification : BaseSpecification<GymSchedule>
    {
        public GymSchedulesByDaySpecification(DayOfWeekEnum dayOfWeek)
            : base(s => s.DayOfWeek == dayOfWeek && s.IsActive)
        {
        }
    }
}