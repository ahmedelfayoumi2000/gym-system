using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Payment : BaseEntity
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; } // المبلغ المدفوع
        public DateTime PaymentDate { get; set; } // تاريخ الدفع
        public string PaymentMethod { get; set; } // طريقة الدفع (Cash, Card)
        public string CreatedByUserId { get; set; } //  المستخدم اللي سجل العملية
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}