namespace GymSystem.DAL.Entities
{
    public class Plan : BaseEntity
    {
        public string PlanName { get; set; }
        public int DurationDays { get; set; } // عدد الأيام
        public decimal Price { get; set; } // السعر
        public bool HasOffer { get; set; } = false; //فيه عرض ساري علي الخطة؟
        public decimal? DiscountedPrice { get; set; }
        public DateTime? ExpireDate { get; set; }

    }
}