using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos
{
  
    public class PlanDto
    {
        public int Id { get; set; }
       
        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan name must be between 2 and 100 characters.")]
        public string PlanName { get; set; }

        [Required(ErrorMessage = "Duration in days is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be a positive number.")]
        public int DurationDays { get; set; }


        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}