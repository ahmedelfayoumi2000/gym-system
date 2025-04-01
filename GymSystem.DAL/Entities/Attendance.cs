using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Attendance : BaseEntity
    {
        public string? UserCode { get; set; }
        public int? MembershipId { get; set; }
        public Membership? Membership { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public string? CreatedByUserId { get; set; } // للموظف اللي سجل الدخول

    }
}
