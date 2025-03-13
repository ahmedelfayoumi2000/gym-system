using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IUserCodeGenerator
    {
        Task<string> GenerateUserCodeAsync(int planId, int currentUserCount);
    }
}
