using System.ComponentModel.DataAnnotations;

namespace HmsNet.UI.Models
{
    public class RoomDto
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Room name is required.")]
        [StringLength(100, ErrorMessage = "Room name cannot exceed 100 characters.")]
        public string RoomName { get; set; }

        [Required(ErrorMessage = "Room type is required.")]
        [StringLength(50, ErrorMessage = "Room type cannot exceed 50 characters.")]
        public string RoomType { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; }
    }
}