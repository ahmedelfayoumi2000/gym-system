using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OrderSpec
{
    public class OrderByIdSpecification : BaseSpecification<Order>
    {
        public OrderByIdSpecification(int orderId)
            : base(o => o.Id == orderId && !o.IsDeleted)
        {
        }
    }
}