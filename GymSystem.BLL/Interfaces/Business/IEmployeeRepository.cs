using GymSystem.BLL.Specifications;
using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IEmployeeRepository
    {
        Task<IReadOnlyList<AppUser>> GetAllWithSpecAsync(ISpecification<AppUser> spec);
        Task<int> GetCountAsync(ISpecification<AppUser> spec);
        Task<AppUser> GetByIdAsync(string id);
    }
}
