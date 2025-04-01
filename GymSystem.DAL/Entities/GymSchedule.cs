using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class GymSchedule : BaseEntity
    {
        public DayOfWeekEnum DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public GroupTypeEnum GroupType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
