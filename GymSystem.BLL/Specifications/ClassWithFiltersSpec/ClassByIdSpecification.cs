using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.ClassWithFiltersSpec
{
    public class ClassByIdSpecification : BaseSpecification<Class>
    {
        public ClassByIdSpecification(int id)
            : base(c => c.Id == id && !c.IsDeleted)
        {
            AddIncludes(c => c.Plan);
        }
    }
}
