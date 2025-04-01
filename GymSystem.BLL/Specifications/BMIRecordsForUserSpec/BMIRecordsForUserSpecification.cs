using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Specifications.BMIRecordsForUserSpec
{
    public class BMIRecordsForUserSpecification : BaseSpecification<BMIRecord>
    {
        public BMIRecordsForUserSpecification(string userId)
            : base(x => x.UserId == userId && !x.IsDeleted)
        {
        }
    }
}