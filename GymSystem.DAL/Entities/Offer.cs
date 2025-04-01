using System;

namespace GymSystem.DAL.Entities
{
    public class Offer : BaseEntity
    {
        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }
        public decimal DiscountedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}