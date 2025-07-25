namespace HmsNet.Models.DTO
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
    }
}
