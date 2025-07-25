using HmsNet.Enums;

namespace HmsNet.Models.DTO
{
    public class ServiceResponse<T>
    {
        public ResponseStatus Status { get; set; } = ResponseStatus.Success;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
