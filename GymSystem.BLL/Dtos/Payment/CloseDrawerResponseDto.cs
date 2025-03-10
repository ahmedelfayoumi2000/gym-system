using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Payment
{
    public class CloseDrawerResponseDto
    {
        public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>(); 
        public decimal TotalAmount { get; set; } 
    }
}
