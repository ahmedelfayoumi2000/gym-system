using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos.Offer
{
    public class OfferDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Plan ID must be a positive integer.")]
        public int PlanId { get; set; }

        [Required(ErrorMessage = "Discounted price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Discounted price must be a non-negative value.")]
        public decimal DiscountedPrice { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }
    }
}