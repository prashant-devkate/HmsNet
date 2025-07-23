namespace HmsNet.Models.DTO
{
    public class ServiceResponse<T>
    {
        public string Status { get; set; } = "Success";
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
