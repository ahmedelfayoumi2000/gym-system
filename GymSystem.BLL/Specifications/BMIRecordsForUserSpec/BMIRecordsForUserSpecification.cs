using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.BMIRecordsForUserSpec
{
    public class BMIRecordsForUserSpecification : BaseSpecification<BMIRecord>
    {
        public BMIRecordsForUserSpecification(string userId)
            : base(x => x.UserId == userId && !x.IsDeleted)
        {
        }
    }
}
