using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos.Equipment
{
    public class EquipmentCreateDto
    {
        [Required(ErrorMessage = "Equipment name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Equipment name must be between 2 and 100 characters.")]
        public string EquipmentName { get; set; }

        public string Description { get; set; }

        [Range(2, int.MaxValue, ErrorMessage = "Count must be greater than 1 if provided.")]
        public int? Count { get; set; }

        [Required(ErrorMessage = "Availability status is required.")]
        public bool IsAvailable { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
    }
}