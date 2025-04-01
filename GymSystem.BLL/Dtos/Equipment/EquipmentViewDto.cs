namespace GymSystem.BLL.Dtos.Equipment
{
    public class EquipmentViewDto
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; }
        public string Description { get; set; }
        public int? Count { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastMaintenanceDate { get; set; }


    }
}