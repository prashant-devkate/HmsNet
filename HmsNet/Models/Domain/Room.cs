
using Org.BouncyCastle.Asn1.X509;

namespace HmsNet.Models.Domain
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
