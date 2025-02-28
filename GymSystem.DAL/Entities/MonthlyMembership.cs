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
    }
}