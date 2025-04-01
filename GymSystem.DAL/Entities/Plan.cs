namespace GymSystem.DAL.Entities
{
    public class Plan : BaseEntity
    {
        public string PlanName { get; set; }
        public int DurationDays { get; set; } 
        public decimal Price { get; set; }
        public bool HasOffer { get; set; } = false; 
        public decimal? DiscountedPrice { get; set; }
        public DateTime? ExpireDate { get; set; }

    }
}