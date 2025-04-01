using System;

namespace GymSystem.BLL.Dtos.Attendance
{
    public class AttendanceDto
    {
        public int? Id { get; set; }
        public string? UserCode { get; set; }
        public DateTime? AttendanceDate { get; set; }
    }
}