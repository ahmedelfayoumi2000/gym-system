using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Payment
{
    public class CloseDrawerResponseDto
    {
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>(); 
        public decimal TotalIncome { get; set; } // إجمالي الدخل 
        public decimal TotalExpenses { get; set; } // إجمالي المصروفات 
        public decimal NetAmount { get; set; } //( الصافي (دخل - مصروفات
    }
}
