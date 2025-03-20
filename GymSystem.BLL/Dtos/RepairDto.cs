using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class RepairDto
    {
        [Required(ErrorMessage = "Equipment ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Equipment ID must be a positive integer")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be a non-negative value")]
        public decimal Cost { get; set; }
    }
}
