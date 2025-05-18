using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Entities.AI
{
    public class ExerciseAi : BaseEntity
    {
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public string Category { get; set; }
        public string UserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
