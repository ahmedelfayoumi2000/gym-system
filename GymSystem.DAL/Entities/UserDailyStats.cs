using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class UserDailyStats : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime Date { get; set; }
        public int CaloriesBurned { get; set; } // السعرات المحروقة
        public int TotalCalories { get; set; }  // إجمالي السعرات المستهدفة
        public int Steps { get; set; }          // عدد الخطوات
        public int TotalSteps { get; set; }     // الهدف من الخطوات
        public int WaterIntake { get; set; }    //كمية المياه
        public int TotalWater { get; set; }     // الهدف من المياه
        public float Weight { get; set; }       // الوزن في اليوم ده
    }
}
