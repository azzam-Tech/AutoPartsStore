namespace AutoPartsStore.Core.Models
{
    /// <summary>
    /// Result pattern implementation for better error handling
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string? ErrorMessage { get; protected set; }
        public string? ErrorCode { get; protected set; }
        public List<string>? Errors { get; protected set; }

        protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static Result Success() => new Result(true);

        public static Result Failure(string errorMessage, string? errorCode = null)
            => new Result(false, errorMessage, errorCode);

        public static Result Failure(List<string> errors, string? errorCode = null)
        {
            var result = new Result(false, errors.FirstOrDefault(), errorCode);
            result.Errors = errors;
            return result;
        }
    }

    /// <summary>
    /// Result pattern with data payload
    /// </summary>
    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        protected Result(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null)
            : base(isSuccess, errorMessage, errorCode)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new Result<T>(true, data);

        public static new Result<T> Failure(string errorMessage, string? errorCode = null)
            => new Result<T>(false, default, errorMessage, errorCode);

        public static new Result<T> Failure(List<string> errors, string? errorCode = null)
        {
            var result = new Result<T>(false, default, errors.FirstOrDefault(), errorCode);
            result.Errors = errors;
            return result;
        }
    }
}
