using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Auth
{
    public interface ITokenService
    {
        Task<(string, RefreshToken)> CreateTokenAsync(AppUser user);
        Task<(string, RefreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);

    }
}
