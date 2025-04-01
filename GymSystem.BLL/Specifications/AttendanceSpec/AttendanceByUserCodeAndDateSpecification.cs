using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.AttendanceSpec
{
    public class AttendanceByUserCodeAndDateSpecification : BaseSpecification<Attendance>
    {
        public AttendanceByUserCodeAndDateSpecification(string userCode, DateTime today)
            : base(a => a.UserCode == userCode && a.AttendanceDate.Date == today.Date)
        {
        }
    }
}
