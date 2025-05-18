using System.Collections.Generic;
using GymSystem.DAL.Entities.Enums.Business;
using Microsoft.AspNetCore.Identity;

namespace GymSystem.DAL.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public Address? Address { get; set; }
        public int UserRole { get; set; } // مثل Admin=1, Trainer=2, Receptionist=3, Member=4
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? ProfileImageName { get; set; }
        public string? UserCode { get; set; }
        public string? Gender { get; set; }
        public uint? Age { get; set; }
        public decimal? Salary { get; set; }
        public bool IsStopped { get; set; } = false;
        public DateTime? StopDate { get; set; }
        public float? Weight { get; set; }
        public float? Height { get; set; }
        public bool? IsProfileConfirmed { get; set; }
        public Goal? Goal { get; set; }
        public FitnessLevel? FitnessLevel { get; set; }
        public int? CaloriesTarget { get; set; } 
        public int? MealsPerDay { get; set; }
        public int? TrainingDaysPerWeek { get; set; } 

        public MembershipType MembershipType { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public Membership? MonthlyMembership { get; set; }
        public int? NutritionPlanId { get; set; }
        public NutritionPlan nutritionPlan { get; set; }
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; }
        public ICollection<BMIRecord> BMIRecords { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
