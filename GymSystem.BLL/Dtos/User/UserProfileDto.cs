using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UserProfileDto
    {
        public string? ImageUrl { get; set; }
        public string FullName { get; set; }
        public uint Age { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Goal { get; set; }
        public string FitnessLevel { get; set; }
        public bool IsProfileConfirmed { get; set; }
        public List<string> Roles { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
