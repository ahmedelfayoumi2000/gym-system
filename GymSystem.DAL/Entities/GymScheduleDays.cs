using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class GymScheduleDays : BaseEntity
    {
        public GymSchedule GymSchedule { get; set; }
        public DayOfWeekEnum DayOfWeek { get; set; }
    }
}
