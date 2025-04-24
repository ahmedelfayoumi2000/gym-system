using GymSystem.DAL.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace GymSystem.DAL.Entities
{
    public class Membership : BaseEntity
    {

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string PhoneNumber { get; set; }
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? HaveDays { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? LastStopDate { get; set; }  // آخر تاريخ تم فيه بدء الإيقاف

        public string? UserCode { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    }
}