using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.Enums.Business
{
    public enum TransactionType
    {
        Payment,      // دفع زي اشتراك مشترك
        Withdrawal,   // سحب زى مصروفات إصلاح معدات
        Refund,       // استرداد فلوس
        Other         // عمليات أخرى
    }
}
    