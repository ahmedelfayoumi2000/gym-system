using GymSystem.DAL.Entities.Identity;
using System;

namespace GymSystem.DAL.Entities
{
    public class DailyAttendance : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }
}