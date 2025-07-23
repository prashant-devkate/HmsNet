namespace HmsNet.Models.Domain
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public Order? Order { get; set; }
        public Item? Item { get; set; }
    }
}
