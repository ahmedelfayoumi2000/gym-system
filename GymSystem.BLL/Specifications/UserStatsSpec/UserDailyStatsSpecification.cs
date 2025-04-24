using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.UserStatsSpec
{
    public class UserDailyStatsSpecification : BaseSpecification<UserDailyStats>
    {
        public UserDailyStatsSpecification(string userId, DateTime date)
            : base(s => s.UserId == userId && s.Date.Date == date.Date)
        {
        }
    }
}
