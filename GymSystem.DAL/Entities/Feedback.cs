using GymSystem.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities
{
    public class Feedback : BaseEntity
    {
        public string? Comments { get; set; }
        public int Rating { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public string TrainerId { get; set; }
    }
}
