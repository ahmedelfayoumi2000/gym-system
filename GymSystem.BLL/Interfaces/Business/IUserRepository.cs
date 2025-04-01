using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByCodeAsync(string userCode);
        Task<AppUser> GetUserByIdAsync(string userId);


    }
}
