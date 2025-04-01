using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.MembershipSpec
{
    public class MembershipByUserCodeSpecification : BaseSpecification<Membership>
    {
        public MembershipByUserCodeSpecification(string userCode)
            : base(m => m.UserCode == userCode)
        {
        }
    }
}