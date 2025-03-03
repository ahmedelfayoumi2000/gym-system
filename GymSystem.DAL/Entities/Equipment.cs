using System;
using System.Collections.Generic;

namespace GymSystem.DAL.Entities
{
    public class Equipment : BaseEntity
    {
        public string EquipmentName { get; set; }
        public string Description { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAvailable { get; set; }




        public ICollection<EquipmentMaintenance> MaintainedByUsers { get; set; } = new List<EquipmentMaintenance>();
        public ICollection<ClassEquipment> UsedInClasses { get; set; } = new List<ClassEquipment>();
        public ICollection<Product> Products { get; set; } = new List<Product>(); 
    
    }

}