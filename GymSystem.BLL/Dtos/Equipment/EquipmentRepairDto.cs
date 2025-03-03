using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Equipment
{
    public class EquipmentRepairDto
    {
        public int EquipmentId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}
