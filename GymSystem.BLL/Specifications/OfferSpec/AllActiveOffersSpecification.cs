using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.OfferSpec
{
    public class AllActiveOffersSpecification : BaseSpecification<Offer>
    {
        public AllActiveOffersSpecification()
            : base(o => o.IsActive)
        {
            AddIncludes(o => o.Plan);
        }
    }
}