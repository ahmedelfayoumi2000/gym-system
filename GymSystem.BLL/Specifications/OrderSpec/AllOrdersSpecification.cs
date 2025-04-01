using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.OrderSpec
{
    public class AllOrdersSpecification : BaseSpecification<Order>
    {
        public AllOrdersSpecification()
            : base(o => !o.IsDeleted)
        {
            AddIncludes(o => o.Product);
            AddIncludes(o => o.CreatedByUser);
        }
    }
}