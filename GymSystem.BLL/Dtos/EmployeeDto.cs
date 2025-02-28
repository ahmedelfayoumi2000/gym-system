    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos
{
    public class EmployeeDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int UserRole { get; set; } // 1=Admin, 2=Trainer, 3=Receptionist
        public string Gender { get; set; }
    }
}
