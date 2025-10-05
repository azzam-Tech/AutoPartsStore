namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// Œÿ√  ⁄«—÷ („À·«: „Õ«Ê·… ≈‰‘«¡ „Ê—œ „ÊÃÊœ »«·›⁄·)
    /// </summary>
    public class ConflictException : AppException
    {
        public string? ConflictingField { get; }
        public object? ConflictingValue { get; }

        public ConflictException(
            string message,
            string? conflictingField = null,
            object? conflictingValue = null,
            string errorCode = "RESOURCE_CONFLICT")
            : base(message, errorCode)
        {
            ConflictingField = conflictingField;
            ConflictingValue = conflictingValue;
        }
    }
}
