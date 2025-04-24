using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.UserStatsSpec
{
    public class UserWeightHistorySpecification : BaseSpecification<UserDailyStats>
    {
        public UserWeightHistorySpecification(string userId, DateTime startDate, DateTime endDate)
            : base(s => s.UserId == userId &&
                            s.Date >= startDate &&
                            s.Date <= endDate &&
                            s.Weight > 0)// بنجيب الأيام اللي فيها وزن فقط
        {
            AddOrderBy(s => s.Date); 
        }
    }
}
