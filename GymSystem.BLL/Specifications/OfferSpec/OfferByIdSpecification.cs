using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OfferSpec
{
    public class OfferByIdSpecification : BaseSpecification<Offer>
    {
        public OfferByIdSpecification(int id)
            : base(o => o.Id == id && o.IsActive)
        {
            AddIncludes(o => o.Plan);
        }
    }
}