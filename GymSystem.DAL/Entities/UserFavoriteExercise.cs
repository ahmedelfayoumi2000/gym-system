using GymSystem.DAL.Entities.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class UserFavoriteExercise : BaseEntity
    {
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public ICollection<Exercise> Exercises { get; set; } = new HashSet<Exercise>();
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}