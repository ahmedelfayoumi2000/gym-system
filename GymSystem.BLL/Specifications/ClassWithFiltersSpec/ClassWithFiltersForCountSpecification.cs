using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.ClassWithFiltersSpec
{
    public class ClassWithFiltersForCountSpecification : BaseSpecification<Class>
    {
        public ClassWithFiltersForCountSpecification(SpecPrams specParams)
            : base(c => !c.IsDeleted)
        {
            ApplySearchFilter(specParams, c => c.MemberName);
        }
    }
}
