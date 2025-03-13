using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Services
{
    public class UserCodeGenerator : IUserCodeGenerator
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserCodeGenerator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<string> GenerateUserCodeAsync(int planId, int currentUserCount)
        {
            var plan = await _unitOfWork.Repository<Plan>().GetByIdAsync(planId);
            var prefix = plan?.PlanName.Length > 0 ? char.ToUpper(plan.PlanName[0]) : 'U';
            return $"{prefix}{currentUserCount + 1}";
        }
    }
}