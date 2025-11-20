namespace salesngin.Models
{
    public record OperationResult(bool Succeeded, string Message = null)
    {
        public static OperationResult Success(string message = null) => new(true, message);
        public static OperationResult Fail(string message) => new(false, message);
    }

    public sealed record OperationResult<T>(bool Succeeded, T Data, string Message = null)
    : OperationResult(Succeeded, Message)
    {
        public static OperationResult<T> Success(T data, string message = null) => new(true, data, message);
        public static new OperationResult<T> Fail(string message) => new(false, default, message);
    }
}
