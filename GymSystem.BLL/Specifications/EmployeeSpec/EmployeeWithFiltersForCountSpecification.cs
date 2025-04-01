using GymSystem.DAL.Entities.Identity;

namespace GymSystem.BLL.Specifications.EmployeeSpec
{
    public class EmployeeWithFiltersForCountSpecification : BaseSpecification<AppUser>
    {
        public EmployeeWithFiltersForCountSpecification(SpecPrams specParams)
            : base(u => (u.UserRole == 1 || u.UserRole == 2 || u.UserRole == 3) && !u.IsDeleted)
        {
            ApplySearchFilter(specParams, u => u.DisplayName);
        }
    }
}