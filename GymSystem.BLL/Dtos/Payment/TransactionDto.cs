using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Payment
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string TransactionType { get; set; } // نوع العملية (Payment, Withdrawal, Refund, Other)
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
    }
}
