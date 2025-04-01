using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Specifications.AttendanceSpec
{
    public class AttendanceWithFiltersForCountSpecification : BaseSpecification<Attendance>
    {
        public AttendanceWithFiltersForCountSpecification(SpecPrams attendanceParams)
            : base()
        {
            ApplyFilter(attendanceParams, attendanceParams.UserCode, a => a.UserCode);
            ApplySearchFilter(attendanceParams, a => a.UserCode);
        }
    }
}