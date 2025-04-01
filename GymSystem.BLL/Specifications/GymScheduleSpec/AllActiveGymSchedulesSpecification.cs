using GymSystem.DAL.Entities;
using GymSystem.BLL.Specifications;

namespace GymSystem.BLL.Specifications.GymScheduleSpec
{
    public class AllActiveGymSchedulesSpecification : BaseSpecification<GymSchedule>
    {
        public AllActiveGymSchedulesSpecification()
            : base(s => s.IsActive)
        {
        }
    }
}