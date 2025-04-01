using GymSystem.BLL.Interfaces.Business;
using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Identity;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Repositories.Business
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IIdentityUserRepository _userRepository;

        public EmployeeRepository(IIdentityUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IReadOnlyList<AppUser>> GetAllWithSpecAsync(ISpecification<AppUser> spec)
        {
            try
            {
                return await _userRepository.GetAllWithSpecAsync(spec);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve employees with specification", ex);
            }
        }

        public async Task<int> GetCountAsync(ISpecification<AppUser> spec)
        {
            try
            {
                return await _userRepository.GetCountAsync(spec);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to count employees with specification", ex);
            }
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Employee ID cannot be null or empty", nameof(id));
            }

            try
            {
                return await _userRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve employee with ID: {id}", ex);
            }
        }
    }
}