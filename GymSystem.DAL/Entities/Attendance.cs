using GymSystem.DAL.Entities.Identity;

namespace GymSystem.DAL.Entities
{
    public class Attendance : BaseEntity
    {
        public bool IsAttended { get; set; }
        public DateTime AttendanceDate { get; set; } = DateTime.Now;
        public DateTime CheckInTime { get; set; }

        public bool IsDeleted { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public string CreatedByUserId { get; set; } // للموظف اللي سجل الدخول
        public AppUser CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // تاريخ التسجيل
        public int ClassId { get; set; }
        public Class Class { get; set; }
    }
}