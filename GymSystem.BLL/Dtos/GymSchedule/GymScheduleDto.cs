using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.GymSchedule
{
    public class GymScheduleDto
    {
        [Required(ErrorMessage = "Day of week is required.")]
        public DayOfWeekEnum DayOfWeek { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Group type is required.")]
        public GroupTypeEnum GroupType { get; set; }
    }
}
