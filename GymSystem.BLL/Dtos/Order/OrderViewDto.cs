using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Order
{
    public class OrderViewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public bool IsAvailable { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
