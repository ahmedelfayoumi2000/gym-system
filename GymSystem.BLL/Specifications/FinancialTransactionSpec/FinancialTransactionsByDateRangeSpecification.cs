using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System;

namespace GymSystem.BLL.Specifications.FinancialTransactionSpec
{
    public class FinancialTransactionsByDateRangeSpecification : BaseSpecification<FinancialTransaction>
    {
        public FinancialTransactionsByDateRangeSpecification(DateTime startDate, DateTime endDate)
            : base(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && !t.IsDeleted)
        {
        }
    }
}