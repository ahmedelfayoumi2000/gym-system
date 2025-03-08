using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Attendance
{
    public class AttendanceViewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CheckInTime { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
