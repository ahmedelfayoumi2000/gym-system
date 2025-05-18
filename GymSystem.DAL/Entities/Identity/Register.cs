using GymSystem.DAL.Entities.Enums.Auth;
using GymSystem.DAL.Entities.Enums.Business;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.DAL.Entities.Identity
{
    //[PlanSelection(ErrorMessage = "You must select either a Daily Plan or a Monthly Plan.")]
    public class Register
    {
        public string DisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
        public string Gender { get; set; }


        public UserRoleEnum UserRole { get; set; }
        public MembershipType MembershipType { get; set; }
    }

 
}