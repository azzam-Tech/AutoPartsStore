namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// Base exception class for all application exceptions
    /// </summary>
    public abstract class AppException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, object>? AdditionalData { get; }

        protected AppException(
            string message,
            string errorCode,
            Dictionary<string, object>? additionalData = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            AdditionalData = additionalData;
        }
    }
}
