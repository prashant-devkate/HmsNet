namespace HmsNet.UI.Models
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int RoomId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailDto>? OrderDetails { get; set; }
        public BillDto? Bill { get; set; }
    }
}
