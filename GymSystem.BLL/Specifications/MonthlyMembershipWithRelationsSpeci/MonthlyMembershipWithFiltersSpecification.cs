using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.IdentityModel.Tokens;

public class MonthlyMembershipWithFiltersSpecification : BaseSpecification<MonthlyMembership>
{
    public MonthlyMembershipWithFiltersSpecification(SpecPrams specParams) : base()
    {
        if (!string.IsNullOrEmpty(specParams.Search))
        {
            Criteria = m => m.User.DisplayName.ToLower().Contains(specParams.Search.ToLower());
        }

        if (!string.IsNullOrEmpty(specParams.Sort))
        {
            switch (specParams.Sort.ToLower())
            {
                case "startdate":
                    AddOrderBy(m => m.StartDate);
                    break;
                case "startdatedesc":
                    AddOrderByDescending(m => m.StartDate);
                    break;
                case "isactive":
                    AddOrderBy(m => m.IsActive);
                    break;
                case "isactivedesc":
                    AddOrderByDescending(m => m.IsActive);
                    break;
                default:
                    AddOrderBy(m => m.Id);
                    break;
            }
        }

        if (specParams.PageSize > 0)
        {
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }

        AddIncludes(m => m.User);
        AddIncludes(m => m.Class);
        AddIncludes(m => m.Plan);
    }
}