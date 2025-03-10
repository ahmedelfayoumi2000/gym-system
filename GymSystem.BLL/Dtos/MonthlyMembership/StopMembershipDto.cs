using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.MonthlyMembership
{
    public class StopMembershipDto
    {
        public string UserCode { get; set; } 
        public int NumberOfDays { get; set; } 
    }
}
