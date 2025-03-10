using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymSystem.DAL.Entities
{
    public class Class : BaseEntity
    {
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }

        public string ClassName { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsDeleted { get; set; }



        public string? TrainerId { get; set; }
        public AppUser Trainer { get; set; }

        //===============================
        public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        //======================

        public ICollection<MonthlyMembership> MonthlyMemberships { get; set; } = new List<MonthlyMembership>();
        public ICollection<DailyAttendance> DailyAttendances { get; set; } = new List<DailyAttendance>();

        public ICollection<ClassEquipment> ClassEquipments { get; set; } = new List<ClassEquipment>();
        //============================================
        public string MemberName { get; set; }
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }


    }
}