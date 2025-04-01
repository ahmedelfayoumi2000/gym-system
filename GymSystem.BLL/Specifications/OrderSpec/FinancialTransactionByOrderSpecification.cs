using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OrderSpec
{
    public class FinancialTransactionByOrderSpecification : BaseSpecification<FinancialTransaction>
    {
        public FinancialTransactionByOrderSpecification(int orderId, string productName, DateTime transactionDate, decimal amount)
            : base(t => t.Description.Contains($"Order payment for Order ID: {orderId}, Product: {productName}")
                        && !t.IsDeleted
                        && t.TransactionDate == transactionDate
                        && t.Amount == amount)
        {
        }
    }
}