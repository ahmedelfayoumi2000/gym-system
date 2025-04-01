using GymSystem.BLL.Dtos.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.MonthlyMembership
{
    public class HomeMembership
    {
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public IEnumerable<MonthlyMembershipViewDto>? Members { get; set; } = new List<MonthlyMembershipViewDto>();
        public int? TotalMembers { get; set; }
        public int? TotalActiveMembers { get; set; }
        public int? TotalSusbendMembers { get; set; }

    }
}
