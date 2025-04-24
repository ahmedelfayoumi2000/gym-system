using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Dashboard
{
    public class AddDailyStatsDto
    {
        public int CaloriesBurned { get; set; }
        public int Steps { get; set; }
        public int WaterIntake { get; set; }
        public float Weight { get; set; }
        public DateTime Date { get; set; } // التاريخ اللي هيضاف فيه الإحصائيات
    }
}
