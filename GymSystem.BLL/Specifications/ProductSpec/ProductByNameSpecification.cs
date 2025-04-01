using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.ProductSpec
{
    public class ProductByNameSpecification : BaseSpecification<Product>
    {
        public ProductByNameSpecification(string name)
            : base(p => p.Name == name && !p.IsDeleted)
        {
        }
    }
}