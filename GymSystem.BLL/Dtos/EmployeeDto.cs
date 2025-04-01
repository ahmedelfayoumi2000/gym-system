using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string? PassWord { get; set; }

        [Required(ErrorMessage = " 1=Admin, 2=Trainer, 3=Receptionist => otherWise =Member")]
        public int UserRole { get; set; } 
        public string? Gender { get; set; }
        public decimal? Salary { get; set; }

    }
}
