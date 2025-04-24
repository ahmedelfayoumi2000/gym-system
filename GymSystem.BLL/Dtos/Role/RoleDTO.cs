using GymSystem.DAL.Entities.Enums.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Dtos.Role
{
    public class RoleDTO
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
