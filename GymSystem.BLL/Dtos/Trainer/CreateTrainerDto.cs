using GymSystem.DAL.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GymSystem.API.DTOs.Trainer
{
    public class CreateTrainerDto
    {
        [Required(ErrorMessage = "Display name is required")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender")]
        public string Gender { get; set; }
        public Address? Address { get; set; }

        [Required(ErrorMessage = "Age is required")]
        public uint Age { get; set; }
        public decimal Salary { get; set; }

    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}