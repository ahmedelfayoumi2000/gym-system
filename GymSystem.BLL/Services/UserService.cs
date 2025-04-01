using GymSystem.BLL.Interfaces;
using GymSystem.BLL.Interfaces.Business;
using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GymSystem.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public Task<AppUser> FindByIdAsync(string userId) => _userManager.FindByIdAsync(userId);
        public Task<AppUser> FindByEmailAsync(string email) => _userManager.FindByEmailAsync(email);
        public Task<IdentityResult> CreateAsync(AppUser user, string password) => _userManager.CreateAsync(user, password);
        public Task<IdentityResult> UpdateAsync(AppUser user) => _userManager.UpdateAsync(user);
        public Task<int> CountAsync() => _userManager.Users.CountAsync();
        public async Task<IdentityResult> AddToRoleAsync(AppUser user, string role) => await _userManager.AddToRoleAsync(user, role);


    }
}