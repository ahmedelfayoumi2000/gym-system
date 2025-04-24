using GymSystem.DAL.Entities.Enums.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.Identity
{
    public class Login
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public Types? Type { get; set; }

    }
}
