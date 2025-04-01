using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos.Equipment
{
    public class EquipmentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Equipment name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Equipment name must be between 2 and 100 characters.")]
        public string EquipmentName { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int Quantity { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }
    }
}