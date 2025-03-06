namespace GymSystem.DAL.Entities
{
    public class Product : BaseEntity
    {
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }

    }   
}