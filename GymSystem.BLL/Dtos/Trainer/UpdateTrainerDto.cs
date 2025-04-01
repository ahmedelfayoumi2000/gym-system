using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Trainer
{
    public class UpdateTrainerDto
    {
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public uint Age { get; set; }
        public Address Address { get; set; }
        public decimal Salary { get; set; }

    }
}
