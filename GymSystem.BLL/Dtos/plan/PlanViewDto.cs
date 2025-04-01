namespace GymSystem.BLL.Dtos.plan
{
    public class PlanViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool HasOffer { get; set; } //فيه عرض ساري علي الخطة؟
        public decimal? DiscountedPrice { get; set; }
        public DateTime? ExpireDate { get; set; }

    }
}