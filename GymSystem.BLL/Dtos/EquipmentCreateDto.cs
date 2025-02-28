using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos
{
    public class EquipmentCreateDto
    {
        [Required(ErrorMessage = "Equipment name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Equipment name must be between 2 and 100 characters.")]
        public string EquipmentName { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Availability status is required.")]
        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Last maintenance date is required.")]
        public DateTime LastMaintenanceDate { get; set; }
    }
}