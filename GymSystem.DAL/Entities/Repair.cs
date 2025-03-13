using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Repair : BaseEntity
    {

        public string? Description { get; set; }
        public decimal Cost { get; set; }

        //===========
        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

    }
}
