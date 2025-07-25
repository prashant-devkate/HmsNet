using HmsNet.UI.Enums;

namespace HmsNet.UI.Models
{
    public class ServiceResponse<T>
    {
        public ResponseStatus Status { get; set; } 
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
