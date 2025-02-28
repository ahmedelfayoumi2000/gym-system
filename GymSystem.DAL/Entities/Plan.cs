namespace GymSystem.DAL.Entities
{
    public class Plan : BaseEntity
    {
        public string PlanName { get; set; }
        public int DurationDays { get; set; } // عدد الأيام
        public decimal Price { get; set; } // السعر
    }
}