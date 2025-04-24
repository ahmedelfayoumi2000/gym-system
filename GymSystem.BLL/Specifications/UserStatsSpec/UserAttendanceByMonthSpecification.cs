using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.UserStatsSpec
{
    public class UserAttendanceByMonthSpecification : BaseSpecification<Attendance>
    {
        public UserAttendanceByMonthSpecification(string userId, int year, int month)
            : base(a => a.UserId == userId &&
                   a.AttendanceDate.Year == year &&
                   a.AttendanceDate.Month == month)
        {

        }
    }
}
