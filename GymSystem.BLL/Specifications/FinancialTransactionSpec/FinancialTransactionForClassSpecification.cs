using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.FinancialTransactionSpec
{
    public class FinancialTransactionForClassSpecification : BaseSpecification<FinancialTransaction>
    {
        public FinancialTransactionForClassSpecification(string memberName, DateTime startTime)
            : base(t => t.Description.Contains($"Class payment for Member: {memberName}")
                        && !t.IsDeleted
                        && t.TransactionDate == startTime)
        {
        }
    }
}
