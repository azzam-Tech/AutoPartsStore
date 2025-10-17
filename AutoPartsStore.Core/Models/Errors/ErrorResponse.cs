namespace AutoPartsStore.Core.Models.Errors
{
    /// <summary>
    /// Standardized error response model
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique error code for client-side handling
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp when error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request path where error occurred
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Trace identifier for logging correlation
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// List of validation errors (if applicable)
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// Field-level validation errors
        /// </summary>
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Additional context data
        /// </summary>
        public Dictionary<string, object>? AdditionalData { get; set; }

        /// <summary>
        /// Stack trace (only in development)
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception details (only in development)
        /// </summary>
        public string? InnerException { get; set; }
    }
}
