using GymSystem.BLL.Dtos;
using GymSystem.BLL.Errors;
using GymSystem.BLL.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Interfaces.Business
{
    public interface IClassRepo
    {
        //crud operations
        public Task<ApiResponse> AddClass(ClassDto classDto);
        Task<ApiResponse> UpdateClass(int id, ClassDto classDto);
        public Task<ApiResponse> DeleteClass(int id);
        public Task<ClassDto> GetClass(int id);
        Task<IEnumerable<ClassDto>> GetClasses(SpecPrams specParams); 


    }
}
