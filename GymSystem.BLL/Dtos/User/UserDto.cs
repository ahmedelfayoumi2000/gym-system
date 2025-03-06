using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
		public string Gender { get; set; }
		public uint? Age { get; set; }
		public List<string> Roles { get; set; } 
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string UserCode { get; set; }
        //public UserRoleEnum Role { get; set; }
    }
}

