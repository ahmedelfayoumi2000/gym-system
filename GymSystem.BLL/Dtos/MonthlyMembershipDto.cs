using System;
using System.ComponentModel.DataAnnotations;

namespace GymSystem.BLL.Dtos
{
    public class MonthlyMembershipDto
    {
      
        public int Id { get; set; }

      
        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "User name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "User name must be between 2 and 100 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Class ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Class ID must be a positive integer.")]
        public int ClassId { get; set; }

       
        public string ClassName { get; set; }

      
        [Required(ErrorMessage = "Plan ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Plan ID must be a positive integer.")]
        public int PlanId { get; set; }

      
        public string PlanName { get; set; }

  
        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

      
        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}