using System.ComponentModel.DataAnnotations;

namespace HmsNet.UI.Models
{
    public class ItemDto
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters.")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Rate must be greater than zero.")]
        public decimal Rate { get; set; }

        public bool IsActive { get; set; }
    }
}
