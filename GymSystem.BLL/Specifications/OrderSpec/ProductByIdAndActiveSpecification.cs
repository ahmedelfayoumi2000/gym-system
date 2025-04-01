using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OrderSpec
{
    public class ProductByIdAndActiveSpecification : BaseSpecification<Product>
    {
        public ProductByIdAndActiveSpecification(int productId)
            : base(p => p.Id == productId && !p.IsDeleted && p.IsActive)
        {
        }
    }
}