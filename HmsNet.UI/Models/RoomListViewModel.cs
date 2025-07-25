namespace HmsNet.UI.Models
{
    public class RoomListViewModel
    {
        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
