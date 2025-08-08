namespace HmsNet.UI.Models
{
    public class ItemListViewModel
    {
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();
        public Dictionary<string, List<RoomDto>> RoomsByType { get; set; } = new Dictionary<string, List<RoomDto>>();
    }
}
