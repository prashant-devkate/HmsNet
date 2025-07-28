using HmsNet.Models.Domain;

namespace HmsNet.Models.DTO
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int RoomId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
