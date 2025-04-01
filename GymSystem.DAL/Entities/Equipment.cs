using System;
using System.Collections.Generic;

namespace GymSystem.DAL.Entities
{
    public class Equipment : BaseEntity
    {
        public string EquipmentName { get; set; }
        public string Description { get; set; }
        public int? Count { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAvailable { get; set; }
    }
}