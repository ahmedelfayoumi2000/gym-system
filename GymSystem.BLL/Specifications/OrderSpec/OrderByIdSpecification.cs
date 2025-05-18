using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using StackExchange.Redis;
using MailKit.Search;

namespace GymSystem.BLL.Specifications.OrderSpec
{
    public class OrderByIdSpecification : BaseSpecification<DAL.Entities.Order>
    {
        public OrderByIdSpecification(int orderId)
            : base(o => o.Id == orderId && !o.IsDeleted)
        {
        }
    }
}