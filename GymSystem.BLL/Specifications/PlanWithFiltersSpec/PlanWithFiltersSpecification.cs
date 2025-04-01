using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Specifications
{
    public class PlanWithFiltersSpecification : BaseSpecification<Plan>
    {
        public PlanWithFiltersSpecification(SpecPrams specParams)
            : base()
        {
            ApplySearchFilter(specParams, p => p.PlanName);

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

            ApplyPagination(specParams);
        }
    }
}