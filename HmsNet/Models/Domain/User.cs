using System.ComponentModel.DataAnnotations;

namespace HmsNet.Models.Domain
{
    public class User
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Firstname { get; set; }

        [Required, StringLength(100)]
        public string Lastname { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}