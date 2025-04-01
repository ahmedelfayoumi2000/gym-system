using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.MembershipSpec
{
    public class MonthlyMembershipWithFiltersSpecification : BaseSpecification<Membership>
    {
        public MonthlyMembershipWithFiltersSpecification(SpecPrams specParams)
            : base(!string.IsNullOrEmpty(specParams.Search)
                  ? m => m.UserCode.Contains(specParams.Search) || m.UserName.Contains(specParams.Search)
                  : null)
        {
            AddIncludes(m => m.User);
            AddIncludes(m => m.Plan);

            ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);


            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "startDateAsc":
                        AddOrderBy(m => m.StartDate);
                        break;
                    case "startDateDesc":
                        AddOrderByDescending(m => m.StartDate);
                        break;
                    case "endDateAsc":
                        AddOrderBy(m => m.EndDate);
                        break;
                    case "endDateDesc":
                        AddOrderByDescending(m => m.EndDate);
                        break;
                    default:
                        AddOrderBy(m => m.StartDate);
                        break;
                }
            }
        }
    }
}