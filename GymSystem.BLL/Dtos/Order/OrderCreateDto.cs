using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Order
{
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than 0.")]
        public int ProductId { get; set; }

        //[Required(ErrorMessage = "Order name is required.")]
        //[StringLength(100, ErrorMessage = "Order name cannot exceed 100 characters.")]
        //public string Name { get; set; }

        //[Required(ErrorMessage = "Price is required.")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        //public decimal Price { get; set; }

        [Required(ErrorMessage = "Count is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0.")]
        public int Count { get; set; }

        //[Required(ErrorMessage = "Availability status is required.")]
        //public bool IsAvailable { get; set; }
    }
}
