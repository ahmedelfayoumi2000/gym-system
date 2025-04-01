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
        public string UserCode { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime AttendanceDate { get; set; }

    }
}
