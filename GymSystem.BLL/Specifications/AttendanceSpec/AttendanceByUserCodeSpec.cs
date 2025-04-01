using GymSystem.DAL.Entities;
using Microsoft.IdentityModel.Tokens;

namespace GymSystem.BLL.Specifications.AttendanceSpec
{
    public class AttendanceByUserCodeSpec : BaseSpecification<Attendance>
    {
        public AttendanceByUserCodeSpec(SpecPrams attendanceParams)
            : base()
        {
            ApplyFilter(attendanceParams, attendanceParams.UserCode, a => a.UserCode);
            ApplySearchFilter(attendanceParams, a => a.UserCode);

            AddIncludes(a => a.Membership);

            AddOrderBy(a => a.AttendanceDate);

            if (!string.IsNullOrEmpty(attendanceParams.Sort))
            {
                switch (attendanceParams.Sort.ToLower())
                {
                    case "dateasc":
                        AddOrderBy(a => a.AttendanceDate);
                        break;
                    case "datedesc":
                        AddOrderByDescending(a => a.AttendanceDate);
                        break;
                    default:
                        AddOrderBy(a => a.AttendanceDate);
                        break;
                }
            }

            ApplyPagination(attendanceParams);
        }

        public AttendanceByUserCodeSpec(string userCode)
            : base(a => a.UserCode == userCode)
        {
            AddIncludes(a => a.Membership);
            AddOrderBy(a => a.AttendanceDate);
        }
    }
}