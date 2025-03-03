namespace GymSystem.BLL.Dtos.Equipment
{
    public class EquipmentViewDto
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public int MaintenanceCount { get; set; } // عدد مرات الصيانة (من MaintainedByUsers)
        public int ClassUsageCount { get; set; } // عدد الحصص التي تستخدم المعدة (من UsedInClasses)
    }
}