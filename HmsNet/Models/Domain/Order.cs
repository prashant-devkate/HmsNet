namespace HmsNet.Models.Domain
{
    public class Order
    {
        public int OrderId { get; set; }
        public int RoomId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public Room? Room { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public Bill? Bill { get; set; }
    }

}
