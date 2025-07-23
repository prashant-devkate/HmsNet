namespace HmsNet.UI.Models
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public List<OrderDto>? Orders { get; set; }
    }
}
