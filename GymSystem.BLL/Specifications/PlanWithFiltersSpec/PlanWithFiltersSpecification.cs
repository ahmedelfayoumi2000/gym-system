using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities;
using Microsoft.IdentityModel.Tokens;

public class PlanWithFiltersSpecification : BaseSpecification<Plan>
{
    public PlanWithFiltersSpecification(SpecPrams specParams) : base()
    {
        if (!string.IsNullOrEmpty(specParams.Search))
        {
            Criteria = p => p.PlanName.ToLower().Contains(specParams.Search.ToLower());
        }

        if (!string.IsNullOrEmpty(specParams.Sort))
        {
            switch (specParams.Sort.ToLower())
            {
                case "price":
                    AddOrderBy(p => p.Price);
                    break;
                case "pricedesc":
                    AddOrderByDescending(p => p.Price);
                    break;
                case "duration":
                    AddOrderBy(p => p.DurationDays);
                    break;
                case "durationdesc":
                    AddOrderByDescending(p => p.DurationDays);
                    break;
                default:
                    AddOrderBy(p => p.Id);
                    break;
            }
        }

        if (specParams.PageSize > 0)
        {
            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}