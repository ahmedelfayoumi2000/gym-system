using GymSystem.DAL.Entities.Enums.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class FinancialTransaction : BaseEntity
    {
        public TransactionType TransactionType { get; set; } // نوع العملية
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } // تاريخ العملية
        public string? Description { get; set; }
        public string? CreatedByUserId { get; set; } //  المستخدم اللي سجل العملية
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}