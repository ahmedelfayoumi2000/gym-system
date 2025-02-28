using System;

namespace GymSystem.BLL.Dtos
{
    public class DailyAttendanceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserCode { get; set; }
        public int ClassId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }
}