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
        public int EquipmentId { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime? Time { get; set; }


    }
}
