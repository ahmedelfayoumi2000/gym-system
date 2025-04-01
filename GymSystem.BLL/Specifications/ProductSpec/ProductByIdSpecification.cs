using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ProductSpec
{
    public class ProductByIdSpecification : BaseSpecification<Product>
    {
        public ProductByIdSpecification(int productId)
            : base(p => p.Id == productId && !p.IsDeleted)
        {
        }
    }
}