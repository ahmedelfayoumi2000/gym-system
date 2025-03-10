using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Trainer
{
    public class TrainerDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public uint Age { get; set; }
        public Address? Address { get; set; }
        //public DateTime StartDate { get; set; }
        //      public DateTime EndDate { get; set; }
        public bool IsStopped { get; set; }
        //public int HaveDays { get; set; }
        public string? AddBy { get; set; }
        public decimal Salary { get; set; }
    }
}
