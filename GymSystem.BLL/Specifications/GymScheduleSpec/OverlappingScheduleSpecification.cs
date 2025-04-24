using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System;
using GymSystem.DAL.Entities.Enums.Business;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class OverlappingScheduleSpecification : BaseSpecification<GymSchedule>
    {
        public OverlappingScheduleSpecification(DayOfWeekEnum day, TimeSpan startTime, TimeSpan endTime)
            : base(s => s.DaysOfWeek.Any(d => d.DayOfWeek == day) &&
                        s.IsActive &&
                        (s.StartTime < endTime && s.EndTime > startTime))
        {
            AddIncludes(s => s.DaysOfWeek);
        }
    }
}