using GymSystem.DAL.Entities.Identity;

namespace GymSystem.BLL.Specifications.EmployeeSpec
{
    public class EmployeeSpecification : BaseSpecification<AppUser>
    {
        public EmployeeSpecification(SpecPrams specParams)
            : base(u => (u.UserRole == 1 || u.UserRole == 2 || u.UserRole == 3) && !u.IsDeleted)
        {
            ApplySearchFilter(specParams, u => u.DisplayName);

            AddOrderBy(u => u.DisplayName);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort.ToLower())
                {
                    case "name":
                        AddOrderBy(u => u.DisplayName);
                        break;
                    case "namedesc":
                        AddOrderByDescending(u => u.DisplayName);
                        break;
                    default:
                        AddOrderBy(u => u.Id);
                        break;
                }
            }

            ApplyPagination(specParams);
        }

        public EmployeeSpecification(string id)
            : base(u => u.Id == id && (u.UserRole == 1 || u.UserRole == 2 || u.UserRole == 3) && !u.IsDeleted)
        {
        }
    }
}