
using JWTUAuthLogin.Shared.Enums;
using JWTUAuthLogin.Shared.Extensions;

namespace JWTUAuthLogin.Shared.Models
{
    public class ServiceActionResult
    {
        public ReturnStatus Status { get; set; }
        public string StatusTerm { get; set; }
        public string Message { get; set; }
        public object? ResultObject { get; set; }

        public ServiceActionResult(ReturnStatus status, string message)
            : this(status)
        {
            Status = status;
            Message = message;
        }

        public ServiceActionResult(ReturnStatus status)
        {
            Status = status;
            StatusTerm = status.ToString();
            Message = status.GetEnumDescription();
            ResultObject = null;
        }
        public ServiceActionResult(ReturnStatus status, string message,object? resultObject)
        {
            Status = status;
            StatusTerm = status.ToString();
            Message = string.IsNullOrEmpty(message) ? status.GetEnumDescription() : message;
            ResultObject = resultObject;
        }
    }
}
