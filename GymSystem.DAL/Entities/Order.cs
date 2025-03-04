using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Order : BaseEntity
    {
        public int ProductId { get; set; } 
        public Product Product { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}