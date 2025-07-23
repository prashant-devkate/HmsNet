namespace HmsNet.Models.Domain
{
    public class Item
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
