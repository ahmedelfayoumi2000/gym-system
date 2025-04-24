using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

public class MonthlyMembershipWithFiltersSpecification : BaseSpecification<Membership>
{
    public MonthlyMembershipWithFiltersSpecification(SpecPrams specParams)
    {
        if (specParams.IsActive.HasValue)
        {
            Criteria = m => m.IsActive == specParams.IsActive.Value;
        }

        if (!string.IsNullOrEmpty(specParams.Search))
        {
            Expression<Func<Membership, bool>> searchCriteria =
                m => m.UserCode.Contains(specParams.Search) || m.UserName.Contains(specParams.Search);
            if (Criteria == null)
            {
                Criteria = searchCriteria;
            }
            else
            {
                Criteria = m => Criteria.Compile()(m) && searchCriteria.Compile()(m);
            }
        }

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