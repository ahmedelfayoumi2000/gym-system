using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System;
using GymSystem.DAL.Entities.Enums.Business;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class OverlappingScheduleExcludingIdSpecification : BaseSpecification<GymSchedule>
    {
        public OverlappingScheduleExcludingIdSpecification(int excludeId, DayOfWeekEnum dayOfWeek, TimeSpan startTime, TimeSpan endTime)
            : base(s =>
                s.Id != excludeId &&
                s.DayOfWeek == dayOfWeek &&
                s.IsActive &&
                ((startTime >= s.StartTime && startTime < s.EndTime) ||
                 (endTime > s.StartTime && endTime <= s.EndTime) ||
                 (startTime <= s.StartTime && endTime >= s.EndTime)))
        {
        }
    }
}