using GymSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.BLL.Specifications.WorkoutPlanSpec
{
    public class WorkoutPlansForMemberSpecification : BaseSpecification<WorkoutPlan>
    {
        public WorkoutPlansForMemberSpecification(int membershipId)
            : base(w => !w.IsDeleted && w.MembershipId == membershipId)
        {
            AddIncludes(w => w.Exercises);
            AddThenInclude(w => w.Exercises, e => e.ExerciseCategory);

            AddOrderBy(w => w.DayOfWeek);
        }
    }
}
