using GymSystem.DAL.Entities.Enums.Business;
using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class CurrentActiveGymScheduleSpecification : BaseSpecification<GymSchedule>
    {
        public CurrentActiveGymScheduleSpecification(TimeSpan currentTime, DayOfWeekEnum currentDay)
            : base(s =>
                s.IsActive &&
                s.DaysOfWeek.Any(d => d.DayOfWeek == currentDay) &&
                s.StartTime <= currentTime &&
                s.EndTime >= currentTime)
        {
            AddIncludes(s => s.DaysOfWeek);
        }
    }
}
