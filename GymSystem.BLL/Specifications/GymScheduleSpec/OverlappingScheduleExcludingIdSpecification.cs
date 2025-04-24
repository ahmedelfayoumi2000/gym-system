using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System;
using GymSystem.DAL.Entities.Enums.Business;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class OverlappingScheduleExcludingIdSpecification : BaseSpecification<GymSchedule>
    {
        public OverlappingScheduleExcludingIdSpecification(int excludeId, DayOfWeekEnum day, TimeSpan startTime, TimeSpan endTime)
            : base(s => s.Id != excludeId &&
                        s.DaysOfWeek.Any(d => d.DayOfWeek == day) &&
                        s.IsActive &&
                        (s.StartTime < endTime && s.EndTime > startTime))
        {
            AddIncludes(s => s.DaysOfWeek);
        }
    }
}