using GymSystem.DAL.Entities.Identity;
using System;

namespace GymSystem.DAL.Entities
{
    public class Membership : BaseEntity
    {
        public string ImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; }

        public string? UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsActive { get; set; }
        public string UserCode { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
    }
}