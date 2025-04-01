using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MembershipSpec
{
    public class MembershipByUserCodeAndActiveSpecification : BaseSpecification<Membership>
    {
        public MembershipByUserCodeAndActiveSpecification(string userCode)
            : base(m => m.UserCode == userCode && m.IsActive)
        {
        }
    }
}