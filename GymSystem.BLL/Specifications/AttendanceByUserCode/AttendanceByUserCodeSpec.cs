using GymSystem.DAL.Entities;
using System;
using System.Linq.Expressions;

namespace GymSystem.BLL.Specifications.AttendanceByUserCode
{
    public class AttendanceByUserCodeSpec : BaseSpecification<Attendance>
    {
        public AttendanceByUserCodeSpec(string userCode)
        {
            Criteria = a => a.User.UserCode == userCode;
            AddIncludes(a => a.Membership);
        }
    }
}