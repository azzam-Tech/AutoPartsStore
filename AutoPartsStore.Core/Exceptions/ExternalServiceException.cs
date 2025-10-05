namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// Œÿ√ ›Ì Œœ„… Œ«—ÃÌ… („À·«: Œœ„… «·œ›⁄° Œœ„… «·≈Ì„Ì·)
    /// </summary>
    public class ExternalServiceException : AppException
    {
        public string? ServiceName { get; }
        public string? ServiceErrorCode { get; }

        public ExternalServiceException(
            string message,
            string? serviceName = null,
            string? serviceErrorCode = null,
            string errorCode = "EXTERNAL_SERVICE_ERROR",
            Exception? innerException = null)
            : base(message, errorCode, null, innerException)
        {
            ServiceName = serviceName;
            ServiceErrorCode = serviceErrorCode;
        }
    }
}
