using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? Count { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;


    }
}
