namespace HmsNet.Models.DTO
{
    public class UpdateProfileDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
