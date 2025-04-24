using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.GymSchedule
{
    public class GymScheduleViewDto
    {
        public int Id { get; set; }
        public List<DayOfWeekEnum> DaysOfWeek { get; set; } = new List<DayOfWeekEnum>();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public GroupTypeEnum GroupType { get; set; }
        public bool IsActive { get; set; }
    }
}
