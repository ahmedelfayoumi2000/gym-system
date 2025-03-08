using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Attendance
{
    public class AttendanceCheckInDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }
    }
}
