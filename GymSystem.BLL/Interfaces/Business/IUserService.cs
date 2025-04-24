using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IUserService
    {
        Task<AppUser> FindByIdAsync(string userId);
        Task<AppUser> FindByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(AppUser user, string password);
        Task<IdentityResult> UpdateAsync(AppUser user);
        Task<int> CountAsync();
        Task<IdentityResult> AddToRoleAsync(AppUser user, string role);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByPhoneNumberAsync(string userName);

    }
}
