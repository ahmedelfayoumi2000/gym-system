using GymSystem.DAL.Entities.Enums.Business;
using Microsoft.AspNetCore.Identity;

namespace GymSystem.DAL.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public Address? Address { get; set; }
        public int UserRole { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? ProfileImageName { get; set; }
        public string? UserCode { get; set; }
        public string? Gender { get; set; }
        public uint? Age { get; set; }
        public decimal? Salary { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => _endDate = value == default ? DateTime.Now : value;
        }

        public bool IsStopped { get; set; } = false;
        public DateTime? StopDate { get; set; }
        public int? RemainingDays
        {
            get
            {
                TimeSpan remainingTime = EndDate - DateTime.Now;
                return remainingTime.Days;
            }
        }
        public float? Weight { get; set; }
        public float? Height { get; set; }

        public bool? IsProfileConfirmed { get; set; }
        public string? Goal { get; set; }
        public string? FitnessLevel { get; set; }

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