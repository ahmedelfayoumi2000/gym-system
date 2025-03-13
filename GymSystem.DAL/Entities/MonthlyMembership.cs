using GymSystem.DAL.Entities.Identity;
using System;

namespace GymSystem.DAL.Entities
{
    public class MonthlyMembership : BaseEntity
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
        public int PlanId { get; set; } 
        public Plan Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }


        //===============================================

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
     
        public int? HaveDays { get; set; }
        public DateTime? StopDate { get; set; }

        // علاقه عشان الموبيل
        public string? UserCode { get; set; }
     
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public DateTime? LastStopDate { get; set; }  // آخر تاريخ تم فيه بدء الإيقاف

      
    }
}