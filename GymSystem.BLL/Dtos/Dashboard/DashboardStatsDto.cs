using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Dashboard
{
    public class DashboardStatsDto
    {
        public List<DateTime> AttendanceDates { get; set; } = new List<DateTime>(); // أيام الحضور
        public int CaloriesBurned { get; set; }
        public int TotalCalories { get; set; }
        public int Steps { get; set; }
        public int TotalSteps { get; set; }
        public int WaterIntake { get; set; }
        public int TotalWater { get; set; }
        public List<WeightEntryDto> WeightHistory { get; set; } = new List<WeightEntryDto>(); // تاريخ الوزن
    }
}
