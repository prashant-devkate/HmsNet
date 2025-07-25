namespace HmsNet.UI.Models
{
    public class ItemListViewModel
    {
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
