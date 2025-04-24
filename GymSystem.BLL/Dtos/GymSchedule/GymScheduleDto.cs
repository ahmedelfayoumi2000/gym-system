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
        [Required(ErrorMessage = "Days of week are required.")]
        public List<DayOfWeekEnum> DaysOfWeek { get; set; } = new List<DayOfWeekEnum>();

        [Required(ErrorMessage = "Start time is required.")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Group type is required.")]
        public GroupTypeEnum GroupType { get; set; }
    }
}
